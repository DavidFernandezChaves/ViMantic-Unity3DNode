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

find_package(darknet_ros_msgs QUIET)
find_package(darknet_ros QUIET)
find_package(mask_rcnn_ros QUIET)


## Generate messages in the 'msg' folder
add_message_files(
  FILES
  SemanticObject.msg
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
)

if(NOT mask_rcnn_ros_FOUND)
    message(STATUS "No he encontrado mask_rcnn")
else()
    catkin_package(
        CATKIN_DEPENDS
        mask_rcnn_ros_msgs
    )
endif()

if(NOT darknet_ros_msgs_FOUND)
    message(STATUS "No he encontrado mask_rcnn")
else()
    catkin_package(
        CATKIN_DEPENDS
        darknet_ros_msgs
        darknet_ros
    )
endif()

include_directories(
# include
  ${catkin_INCLUDE_DIRS}
)


