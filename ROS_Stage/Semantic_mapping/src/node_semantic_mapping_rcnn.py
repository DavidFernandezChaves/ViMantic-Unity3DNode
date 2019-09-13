#! /usr/bin/env python
import sys
import cv2
import mask_rcnn_ros.msg
import message_filters
import numpy as np
import rospy
import std_msgs.msg
import tf2_geometry_msgs
import tf2_ros
from cv_bridge import CvBridge, CvBridgeError
from geometry_msgs.msg import Point32, PoseStamped, Point, Vector3
from semantic_mapping.msg import SemanticObject
from sensor_msgs.msg import Image
from sensor_msgs.msg import PointCloud
from sympy.printing.tests.test_tensorflow import tf

bridge = CvBridge()
image = []
debug = None
waiting_answer = False
img_angle = 0
time_stamp = 0
transform = None
threshold = 0

cx = 314.649173 / 2
cy = 240.160459 / 2
fx = 572.882768
fy = 542.739980

pub_pose = None


def semantic_mapping_rcnn(self):
    global image, debug, point_cloud, img_angle, threshold, pub_pose

    rospy.init_node('semantic_mapping', anonymous=True)

    # Get Global Parameters
    img_angle = rospy.get_param('~input_angle', 0)
    threshold = rospy.get_param('~threshold', 0.5)
    debug = rospy.get_param('~debug', False)
    point_cloud = rospy.get_param('~point_cloud', False)

    tfBuffer = tf2_ros.Buffer()
    listener = tf2_ros.TransformListener(tfBuffer)

    # Publisher
    pub_result = rospy.Publisher(rospy.get_param('~topic_result', 'semantic_mapping/semantic_object'), SemanticObject,
                                 queue_size=10)
    pub_repub = rospy.Publisher('semantic_mapping/RGB_image_detected', Image,
                                queue_size=10)
    pub_pose = rospy.Publisher('semantic_mapping/point', PoseStamped, queue_size=10)

    # Subscribers
    rospy.Subscriber(rospy.get_param('~topic_cnn'), mask_rcnn_ros.msg.Result, callback_new_detection, pub_result)
    sub_depth_image = message_filters.Subscriber(rospy.get_param('~topic_depth'), Image)
    sub_rgb_image = message_filters.Subscriber(rospy.get_param('~topic_intensity'), Image)

    message_filter = message_filters.ApproximateTimeSynchronizer([sub_depth_image, sub_rgb_image], 10, 0.3)
    message_filter.registerCallback(callback_newImage, [pub_repub, tfBuffer])

    tf.logging.set_verbosity(tf.logging.ERROR)

    rospy.spin()


def callback_newImage(depth_data, rgb_data, arg):
    global image, img_angle, waiting_answer, time_stamp, transform, threshold, stamp, debug

    pub_repub = arg[0]
    tfBuffer = arg[1]

    # The corrected rgb image is obtained
    try:
        img_rgb = bridge.imgmsg_to_cv2(rgb_data, "rgb8")
    except CvBridgeError, e:
        print(e)

    if not waiting_answer or depth_data.header.stamp.secs > (time_stamp.secs + 5):
        transform = tfBuffer.lookup_transform("map",
                                              rgb_data.header.frame_id,  # source frame
                                              rospy.Time(0),  # get the tf at first available time
                                              rospy.Duration(5))
        img_rgb = rotate_image(img_rgb, img_angle)

        if debug :
            pub_repub.publish(CvBridge().cv2_to_imgmsg(img_rgb, 'rgb8'))

        # Save the time stamp
        stamp = depth_data.header.stamp
        image = [img_rgb, depth_data]
        waiting_answer = True
        # Save the time stamp
        time_stamp = depth_data.header.stamp


def callback_new_detection(result_cnn, pub_result):
    global image, img_angle, waiting_answer, transform, time_stamp, pub_pose, debug

    # The detected objects are processed
    if (not result_cnn is None) and len(result_cnn.class_names) > 0:

        img_rgb = image[0]
        img_depth = image[1]

        # The corrected depth image and pose transformation are obtained
        try:
            data_depth = bridge.imgmsg_to_cv2(img_depth, "16UC1")
        except CvBridgeError, e:
            print(e)

        # I transform the value of each px to m by acquiring a cloud of points
        data_depth = data_depth / 6553.5
        data_depth = rotate_image(data_depth, img_angle)

        rows, cols = data_depth.shape
        c, r = np.meshgrid(np.arange(cols), np.arange(rows), sparse=True)

        cx, cy = tuple(np.array(data_depth.shape[1::-1]) / 2)

        z = data_depth
        x = ((cx - c) * z / fx)
        y = ((cy - r) * z / fy)

        # Cut out every object from the point cloud and build the result.
        result = SemanticObject()
        pointCloud = PointCloud()
        result.header = std_msgs.msg.Header()
        result.header.stamp = time_stamp
        result.header.frame_id = "/map"
        pointCloud.header = std_msgs.msg.Header()
        pointCloud.header.stamp = time_stamp
        pointCloud.header.frame_id = img_depth.header.frame_id

        for i in range(len(result_cnn.class_names)):
            if result_cnn.scores[i] > threshold:

                # Debug----------------------------------------------------------------------------------------
                if debug:
                    print result_cnn.class_names[i] + ": " + str(result_cnn.scores[i])
                # ---------------------------------------------------------------------------------------------

                try:
                    mask = (bridge.imgmsg_to_cv2(result_cnn.masks[i], "8UC1") == 255)
                except CvBridgeError, e:
                    print(e)

                result.accuracy_estimation = result_cnn.scores[i]
                result.id = result_cnn.class_names[i]

                if len(mask) == 0:
                    return

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
                    return

                # pointCloud.channels = [ChannelFloat32("red", img_rgb[mask, 0]),
                #                        ChannelFloat32("green", img_rgb[mask, 1]),
                #                        ChannelFloat32("blue", img_rgb[mask, 2])]

                scale_x = x_.max() - x_.min()
                scale_y = y_.max() - y_.min()
                scale_z = np.std(z_)

                # z_.max() - z_.min()

                result.scale = Vector3(scale_x, scale_y, scale_z)

                # Calculate the center px
                x_center = int(result_cnn.boxes[i].x_offset + result_cnn.boxes[i].width / 2)
                y_center = int(result_cnn.boxes[i].y_offset + result_cnn.boxes[i].height / 2)
                # And the depth of the center
                z_center = -(float(scale_z / 2) + np.average(z_))

                # Transformed the center of the object to the map reference system
                p1 = PoseStamped()
                p1.header.frame_id = img_depth.header.frame_id
                p1.header.stamp = stamp

                p1.pose.position = Point(-x[y_center, x_center], y[y_center, x_center], z_center)
                p1.pose.orientation.w = 1.0  # Neutral orientation
                pose = tf2_geometry_msgs.do_transform_pose(p1, transform)
                result.pose = pose

                pub_pose.publish(pose)

                if point_cloud:
                    for j in range(len(z_)):
                        pointCloud.points.append(
                            Point32(-round(x_[j] - x_center, 4), round(y_[j] - y_center, 4),
                                    -round(z_[j] - z_center, 4)))

                result.pointCloud = pointCloud

                pub_result.publish(result)

    waiting_answer = False


def rotate_image(image, angle):
    image_center = tuple(np.array(image.shape[1::-1]) / 2)
    rot_mat = cv2.getRotationMatrix2D(image_center, angle, 1.0)
    result = cv2.warpAffine(image, rot_mat, image.shape[1::-1], flags=cv2.INTER_LINEAR)
    return result


if __name__ == '__main__':
    semantic_mapping_rcnn(sys.argv)
