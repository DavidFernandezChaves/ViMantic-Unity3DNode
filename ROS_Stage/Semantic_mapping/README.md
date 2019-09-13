#Parameters
```bash
        #Name of the topic where the results are published
        <param name="topic_result" value="semantic_map/detection"/>
        #Name of the topic where the RGB image is obtained
	    <param name="topic_intensity" value="RGBD_4_intensity"/>
        #Name of the topic where the depth image is obtained
        <param name="topic_depth" value="RGBD_4_depth"/>
        #Threshold of belief to publish a detected object
        <param name="threshold" value="0.7"/>
        #Angle of the input image
        <param name="input_angle" value="90"/>
        #The debug mode allows you to debug the detections by publishing in semantic_maping / debug_ an image of the detected objects and the calculated center coordinates of the object.
	    <param name="debug" value="false"/>
   ```
