#! /usr/bin/env python
import cv2
import random
import sys
import actionlib
import darknet_ros_msgs.msg
import message_filters
import numpy as np
import rospy
import std_msgs.msg
import tf2_geometry_msgs
import tf2_ros
from cv_bridge import CvBridge, CvBridgeError
from geometry_msgs.msg import Point32, PoseStamped, Point
from semantic_mapping.msg import Detection
from sensor_msgs.msg import Image, ChannelFloat32
from sensor_msgs.msg import PointCloud

bridge = CvBridge()
image = []
debug = None

cx = 314.649173 / 2
cy = 240.160459 / 2
fx = 572.882768
fy = 542.739980


def semantic_mapping(self):
    global image, debug

    rospy.init_node('semantic_mapping', anonymous=True)

    # Get Global Parameters
    img_angle = rospy.get_param('~input_angle', 0)
    threshold = rospy.get_param('~threshold', 0.5)
    debug = rospy.get_param('~debug', False)

    # Action Client
    client = actionlib.SimpleActionClient('darknet_ros/check_for_objects', darknet_ros_msgs.msg.CheckForObjectsAction)
    client.wait_for_server()

    # Publisher
    pub_result = rospy.Publisher(rospy.get_param('~topic_result', 'semantic_mapping/detection'), Detection,
                                 queue_size=10)
    pub_result_img = None
    pub_result_point = None
    if debug:
        pub_result_img = rospy.Publisher('semantic_mapping/debug_img', Image, queue_size=10)
        pub_result_point = rospy.Publisher('semantic_mapping/debug_point', PoseStamped, queue_size=10)

    # Subscribers
    depth_image_sub = message_filters.Subscriber(rospy.get_param('~topic_depth'), Image)
    rgb_image_sub = message_filters.Subscriber(rospy.get_param('~topic_intensity'), Image)
    ts = message_filters.ApproximateTimeSynchronizer([depth_image_sub, rgb_image_sub], 10, 0.3)
    ts.registerCallback(callback_newImage)

    while not rospy.is_shutdown():
        tfBuffer = tf2_ros.Buffer()
        listener = tf2_ros.TransformListener(tfBuffer)

        if len(image) > 0:
            core(image, img_angle, threshold, [client, pub_result, tfBuffer, pub_result_img, pub_result_point])


def callback_newImage(depth_data, rgb_data):
    global image
    image = [rgb_data, depth_data]


def core(img_rgbd, img_angle, threshold, arg):
    global debug
    rgb_data = img_rgbd[0]
    depth_data = img_rgbd[1]

    client = arg[0]
    pub_result = arg[1]
    tfBuffer = arg[2]
    pub_result_img = arg[3]
    pub_result_point = arg[4]

    # Save the time stamp
    stamp = depth_data.header.stamp

    # The corrected rgb image is obtained
    try:
        img_rgb = bridge.imgmsg_to_cv2(rgb_data, "bgr8")
    except CvBridgeError, e:
        print(e)

    img_rgb = rotateImage(img_rgb, img_angle)

    # It is sent to CNN and results are expected
    id = random.randint(1, 101)
    imgmsg_rgb = CvBridge().cv2_to_imgmsg(img_rgb, 'bgr8')
    goal = darknet_ros_msgs.msg.CheckForObjectsGoal(id=id, image=imgmsg_rgb)

    t = rospy.get_time()

    ans = darknet_ros_msgs.msg.CheckForObjectsResult(id=0)

    try:
        while not (ans.id == id or rospy.is_shutdown()):
            client.send_goal(goal)
            client.wait_for_result(rospy.Duration.from_sec(3.0))
            ans = client.get_result()
    except:
        None

    client.cancel_all_goals()

    # if debug:
    #     print "Response time: " + str(rospy.get_time() - t)

    # The detected objects are processed
    if (not ans is None) and len(ans.bounding_boxes.bounding_boxes) > 0:

        # The corrected depth image and pose transformation are obtained
        try:
            img_depth = bridge.imgmsg_to_cv2(depth_data, "16UC1")
            transform = tfBuffer.lookup_transform("map",
                                                  rgb_data.header.frame_id,  # source frame
                                                  rospy.Time(0),  # get the tf at first available time
                                                  rospy.Duration(2.0))  # wait for 1 second
        except CvBridgeError, e:
            print(e)

        # I transform the value of each px to m by acquiring a cloud of points
        img_depth = img_depth / 6553.5
        img_depth = rotateImage(img_depth, 90)

        rows, cols = img_depth.shape
        c, r = np.meshgrid(np.arange(cols), np.arange(rows), sparse=True)

        cx, cy = tuple(np.array(img_depth.shape[1::-1]) / 2)

        z = img_depth
        x = (cx - c) * z / fx
        y = (cy - r) * z / fy

        # Cut out every object from the point cloud and build the result.
        result = Detection()
        pointCloud = PointCloud()
        result.header = std_msgs.msg.Header()
        result.header.stamp = stamp
        result.header.frame_id = "/map"
        pointCloud.header = std_msgs.msg.Header()
        pointCloud.header.stamp = stamp
        pointCloud.header.frame_id = depth_data.header.frame_id

        for box in ans.bounding_boxes.bounding_boxes:
            if box.probability > threshold:

                # Debug----------------------------------------------------------------------------------------
                if debug:
                    cv2.rectangle(img_rgb, (box.xmin, box.ymin), (box.xmax, box.ymax), (0, 255, 0), 3)
                    print box.Class + ": " + str(box.probability)
                # ---------------------------------------------------------------------------------------------

                result.probability = box.probability
                result.name = box.Class

                # Cut out the point cloud
                xobj = x[box.xmin:box.xmax, box.ymin:box.ymax]
                yobj = y[box.xmin:box.xmax, box.ymin:box.ymax]
                zobj = z[box.xmin:box.xmax, box.ymin:box.ymax]

                # pointCloud.channels = [ChannelFloat32("red", img_rgb[box.ymin:box.ymax, box.xmin:box.xmax, 0]),
                #                        ChannelFloat32("green", img_rgb[box.ymin:box.ymax, box.xmin:box.xmax, 1]),
                #                        ChannelFloat32("blue", img_rgb[box.ymin:box.ymax, box.xmin:box.xmax, 2])]

                # Packaging
                xobj = xobj.flatten('F')
                yobj = yobj.flatten('F')
                zobj = zobj.flatten('F')

                # Check that some px is detected well so as not to send empty messages
                if len(zobj) > 0:
                    # Get the center of the object
                    x_center = int(box.xmin + (box.xmax - box.xmin) / 2)
                    y_center = int(box.ymin + (box.ymax - box.ymin) / 2)

                    # Mode 1: all point
                    # z_center = min(zobj) + ((max(zobj) - min(zobj)) / 2)

                    # Mode 2: averange
                    z_center = sum(zobj) / len(zobj)

                    # Transformed the center of the object to the map reference system
                    p1 = PoseStamped()
                    p1.header.frame_id = rgb_data.header.frame_id
                    p1.header.stamp = stamp
                    # ???? why -x and -z?
                    p1.pose.position = Point(-x[y_center, x_center], y[y_center, x_center], -z_center)
                    p1.pose.orientation.w = 1.0  # Neutral orientation
                    result.position = tf2_geometry_msgs.do_transform_pose(p1, transform)

                    # Packaging with 25% of depth
                    depth_threshold = (max(zobj) - min(zobj)) * 0.25

                    for i in range(0, len(zobj)):
                        if (z_center - depth_threshold) < zobj[i] < (z_center + depth_threshold):
                            pointCloud.points.append(Point32(xobj[i], yobj[i], zobj[i]))

                    result.pointCloud = pointCloud

                    pub_result.publish(result)

                    if debug:
                        pub_result_point.publish(result.position)

        if debug:
            imgmsg_rgb = CvBridge().cv2_to_imgmsg(img_rgb, 'bgr8')
            pub_result_img.publish(imgmsg_rgb)


def rotateImage(image, angle):
    image_center = tuple(np.array(image.shape[1::-1]) / 2)
    rot_mat = cv2.getRotationMatrix2D(image_center, angle, 1.0)
    result = cv2.warpAffine(image, rot_mat, image.shape[1::-1], flags=cv2.INTER_LINEAR)
    return result


if __name__ == '__main__':
    semantic_mapping(sys.argv)
