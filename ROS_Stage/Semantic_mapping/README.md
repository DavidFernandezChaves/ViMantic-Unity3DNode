# Semantic mapping - ROS Stage

Parameters

        #Name of the topic where the results are published
        <param name="topic_result" value="semantic_mapping/SemanticObject"/>
        
        #Topic name where the RGB image is obtained
	    <param name="topic_intensity" value="RGBD_4_intensity"/>
	    
        #Topic name where the depth image is obtained
        <param name="topic_depth" value="RGBD_4_depth"/>
        
        #Topic name of CNN input
        <param name="topic_republic" value="semantic_mapping/RGB"/>
        
        #Topic name of CNN results
        <param name="topic_cnn" value="mask_rcnn/result"/>
        
        #Threshold of accuracy_estimation to publish a detected object        
        <param name="threshold" value="0.95"/>
        
        #Angle of the input image
        <param name="input_angle" value="90"/>
        
        #Enables the sending of the object's point cloud. (Disable in case of slow wireless networks)
        <param name="point_cloud" value="false"/>        
        
        #Enables debug mode
	    <param name="debug" value="true"/>
	  
	  
# Tips
	    
If using Mask R-CNN gives you this error for compatibility:
   
     TypeError: integer argument expected, got float

It can be solved by removing the following lines from the file "/src/mask_rcnn_ros/utils.py":

    # Resize image and mask
    if scale != 1:
        image = scipy.misc.imresize(
            (int(round(h * scale)), int(round(w * scale))))
