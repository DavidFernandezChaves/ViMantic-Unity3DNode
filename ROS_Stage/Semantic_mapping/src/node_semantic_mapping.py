#! /usr/bin/env python
import sys
import threading

import cv2
import detectron2_ros.msg
import message_filters
import numpy as np
import rospy
import std_msgs.msg
import tf2_geometry_msgs
import tf2_ros
from cv_bridge import CvBridge, CvBridgeError
from geometry_msgs.msg import Point32, PoseStamped, Point, Vector3
from semantic_mapping.msg import SemanticObject, SemanticObjects
from sensor_msgs.msg import Image
from sensor_msgs.msg import PointCloud


class SemanticMappingNode(object):
    def __init__(self):
        self._bridge = CvBridge()

        self._cx = 314.649173 / 2
        self._cy = 240.160459 / 2
        self._fx = 572.882768
        self._fy = 542.739980

        self._img_angle = rospy.get_param('~input_angle', 0)
        self._threshold = rospy.get_param('~threshold', 0.5)
        self._debug = rospy.get_param('~debug', False)
        self._point_cloud_enabled = rospy.get_param('~point_cloud', False)
        self._publish_rate = rospy.get_param('~publish_rate', 100)

        self._las_img_rgb = None
        self._last_msg = None
        self._cnn_msg = None
        self._msg_lock = threading.Lock()
        self._waiting_cnn = False

        self._tfBuffer = tf2_ros.Buffer()
        tf2_ros.TransformListener(self._tfBuffer)

        self._pub_result = rospy.Publisher(rospy.get_param('~topic_result', 'semantic_mapping/SemanticObjects'),
                                           SemanticObjects,
                                           queue_size=10)

        # Publisher
        self._pub_repub = rospy.Publisher(rospy.get_param('~topic_republic', 'semantic_mapping/RGB'), Image,
                                          queue_size=10)
        self._pub_pose = rospy.Publisher('semantic_mapping/point', PoseStamped, queue_size=10)

        # Subscribers
        rospy.Subscriber(rospy.get_param('~topic_cnn'), detectron2_ros.msg.Result, self.callback_new_detection)
        sub_depth_image = message_filters.Subscriber(rospy.get_param('~topic_depth'), Image)
        sub_rgb_image = message_filters.Subscriber(rospy.get_param('~topic_intensity'), Image)

        message_filter = message_filters.ApproximateTimeSynchronizer([sub_depth_image, sub_rgb_image], 10, 0.3)
        message_filter.registerCallback(self._image_callback)

    def run(self):

        rate = rospy.Rate(self._publish_rate)
        while not rospy.is_shutdown():
            #Republish last img
            if self._las_img_rgb is not None:
                self._pub_repub.publish(self._bridge.cv2_to_imgmsg(self._las_img_rgb, 'rgb8'))

            if not self._waiting_cnn and self._msg_lock.acquire(False):
                last_msg = self._last_msg
                self._last_msg = None
                self._msg_lock.release()
            else:
                rate.sleep()
                continue

            if last_msg is not None :
                # The detected objects are processed
                if len(self._cnn_msg.class_names) > 0:

                    img_depth = last_msg[0]
                    data_header = last_msg[1]
                    data_transform = last_msg[2]

                    # Transform the value of each px to m by acquiring a cloud of points
                    img_depth = img_depth / 6553.5
                    img_depth = self.rotate_image(img_depth, self._img_angle)

                    rows, cols = img_depth.shape
                    c, r = np.meshgrid(np.arange(cols), np.arange(rows), sparse=True)

                    z = img_depth
                    x = ((self._cx - c) * z / self._fx)
                    y = ((self._cy - r) * z / self._fy)

                    # Cut out every object from the point cloud and build the result.
                    result = SemanticObjects()

                    result.header = data_header
                    result.header.frame_id = "/map"

                    for i in range(len(self._cnn_msg.class_names)):

                        if self._cnn_msg.scores[i] > self._threshold:
                            semanticObject = SemanticObject()
                            pointCloud = PointCloud()
                            pointCloud.header = data_header

                            semanticObject.score = self._cnn_msg.scores[i]
                            semanticObject.id = self._cnn_msg.class_names[i]

                            try:
                                mask = (self._bridge.imgmsg_to_cv2(self._cnn_msg.masks[i])== 255)
                            except CvBridgeError as e:
                                print(e)

                            x_ = x[mask]
                            y_ = y[mask]
                            z_ = z[mask]

                            # Bandpass filter with Z data
                            top_margin = (z_.max() - z_.min()) * 0.9 + z_.min()
                            bottom_margin = (z_.max() - z_.min()) * 0.1 + z_.min()

                            mask2 = np.logical_and(z_ > bottom_margin, z_ < top_margin)

                            x_ = x_[mask2]
                            y_ = y_[mask2]
                            z_ = z_[mask2]

                            if len(x_) == 0:
                                continue

                            # pointCloud.channels = [ChannelFloat32("red", img_rgb[mask, 0]),
                            #                        ChannelFloat32("green", img_rgb[mask, 1]),
                            #                        ChannelFloat32("blue", img_rgb[mask, 2])]

                            scale_x = x_.max() - x_.min()
                            scale_y = y_.max() - y_.min()
                            scale_z = np.std(z_)

                            # z_.max() - z_.min()

                            semanticObject.scale = Vector3(scale_x, scale_y, scale_z)

                            # Calculate the center px
                            x_center = int(self._cnn_msg.boxes[i].x_offset + self._cnn_msg.boxes[i].width / 2)
                            y_center = int(self._cnn_msg.boxes[i].y_offset + self._cnn_msg.boxes[i].height / 2)
                            # And the depth of the center
                            z_center = -(float(scale_z / 2) + np.average(z_))

                            # Transformed the center of the object to the map reference system
                            p1 = PoseStamped()
                            p1.header = data_header

                            p1.pose.position = Point(-x[y_center, x_center], y[y_center, x_center], z_center)
                            p1.pose.orientation.w = 1.0  # Neutral orientation
                            pose = tf2_geometry_msgs.do_transform_pose(p1, data_transform)
                            semanticObject.pose = pose

                            self._pub_pose.publish(pose)

                            if self._point_cloud_enabled:
                                for j in range(len(z_)):
                                    pointCloud.points.append(
                                        Point32(-round(x_[j] - x_center, 4), round(y_[j] - y_center, 4),
                                                -round(z_[j] - z_center, 4)))

                            semanticObject.pointCloud = pointCloud
                            result.semanticObjects.append(semanticObject)

                            # Debug----------------------------------------------------------------------------------------
                            if self._debug:
                                print (self._cnn_msg.class_names[i] + ": " + str(self._cnn_msg.scores[i]))
                            # ---------------------------------------------------------------------------------------------

                    self._pub_result.publish(result)

            rate.sleep()

    def _image_callback(self, depth_msg, rgb_msg):

        if not self._waiting_cnn and self._msg_lock.acquire(False):
            try:
                img_rgb = self._bridge.imgmsg_to_cv2(rgb_msg, "rgb8")
                img_depth = self._bridge.imgmsg_to_cv2(depth_msg, "16UC1")
            except CvBridgeError as e:
                print(e)

            self._las_img_rgb = self.rotate_image(img_rgb, self._img_angle)

            transform = self._tfBuffer.lookup_transform("map",
                                                        rgb_msg.header.frame_id,  # source frame
                                                        rospy.Time(0),  # get the tf at first available time
                                                        rospy.Duration(5))

            self._last_msg = [img_depth, depth_msg.header, transform]
            self._waiting_cnn = True
            self._msg_lock.release()


    def callback_new_detection(self, result_cnn):
        if self._waiting_cnn and self._msg_lock.acquire(False):
            self._cnn_msg = result_cnn
            self._waiting_cnn = False
            self._msg_lock.release()

    @staticmethod
    def rotate_image(img, angle):
        image_center = tuple(np.array(img.shape[1::-1]) / 2)
        rot_mat = cv2.getRotationMatrix2D(image_center, angle, 1.0)
        result = cv2.warpAffine(img, rot_mat, img.shape[1::-1], flags=cv2.INTER_LINEAR)
        return result


def main(argv):
    rospy.init_node('semantic_mapping')
    node = SemanticMappingNode()
    node.run()

if __name__ == '__main__':
    main(sys.argv)
