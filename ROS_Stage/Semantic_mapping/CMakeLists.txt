cmake_minimum_required(VERSION 2.8.3)
project(semantic_mapping)


find_package(catkin REQUIRED COMPONENTS
  rospy
  std_msgs
  geometry_msgs
  sensor_msgs
  message_generation
  genmsg
  actionlib_msgs
  actionlib
)

find_package(mask_rcnn_ros QUIET)


## Generate messages in the 'msg' folder
add_message_files(
  FILES
  SemanticObject.msg
  SemanticObjects.msg
)

## Generate added messages and services with any dependencies listed here
generate_messages(
  DEPENDENCIES
  std_msgs
  geometry_msgs
  sensor_msgs
  actionlib_msgs
)

## DEPENDS: system dependencies of this project that dependent projects also need
catkin_package(
   CATKIN_DEPENDS
   geometry_msgs
   std_msgs
   sensor_msgs
   message_runtime
   mask_rcnn_ros_msgs
)

include_directories(
# include
  ${catkin_INCLUDE_DIRS}
)


