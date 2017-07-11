/*
#////////////////////////////////////////////////////////////////////////
#
# The MIT License
#
# Copyright (c) 2016 Centre for Research & Technology Hellas
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
#
#        Created By          : K.C. Apostolakis
#        Creation Date       : 20162501
#        Created for Project : ProsocialLearn
#
#////////////////////////////////////////////////////////////////////////
*/

CERTH_Trackr = {
	ctrack:null,
	video:null,
	needsupdate:null,
	frame_width:null,
	frame_height:null,
	canvas:null,
	context:null,
	socket:null,
	
	
	// user id and game instance id
	uuid_user:'',
	uuid_game:'',
	
	// emotion recognition data
	emotionClassifier:null,
	emotionData:null,
	emotionResults:null,
	
	currentVA:null,
	
	// initialization
	init:function(video,width,height,canvas,context, userID, gameID){
		// initialize getUserMedia
		window.URL = window.URL || window.webkitURL,
		navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia,
		
		// initialize components of CERTH_Trackr
		this.ctrack=new clm.tracker({useWebGL:!0}),
		this.ctrack.init(pModel),
		this.video=video,
		this.frame_width=width,
		this.frame_height=height,
		this.canvas=canvas,
		this.context=context,
		
		this.uuid_user=userID;
		this.uuid_game=gameID;
		
		this.emotionClassifier = new emotionClassifier();
		this.emotionData = this.emotionClassifier.getBlank();
		this.emotionClassifier.init(emotionModel);
	},
	
	// starting the tracker
	start:function(){
		navigator.getUserMedia&&navigator.getUserMedia(
			{
				video:!0
			}, function(e) {
				CERTH_Trackr.video.src = window.URL.createObjectURL(e),
				CERTH_Trackr.needsupdate=!0,
				
				CERTH_Trackr.ctrack.start(CERTH_Trackr.video),
				CERTH_Trackr.track()
			}, function(e) {
				console.log("failed to obtain video! - please use GUI and images instead",e)
			}
		)
	},
	
	// stopping the tracker
	stop:function(){
		CERTH_Trackr.ctrack.stop(),
		CERTH_Trackr.needsupdate=!1
	},
	
	// tracking frame
	track:function(){
		null != CERTH_Trackr.canvas && (
			// clear canvas and draw
			CERTH_Trackr.context.clearRect(0,0,CERTH_Trackr.frame_width,CERTH_Trackr.frame_height),
			CERTH_Trackr.context.drawImage(CERTH_Trackr.video,0,0,CERTH_Trackr.frame_width,CERTH_Trackr.frame_height),
			CERTH_Trackr.context.putImageData(CERTH_Trackr.context.getImageData(0,0,CERTH_Trackr.frame_width,CERTH_Trackr.frame_height),0,0)
		),
		
		// draw shape
		CERTH_Trackr.needsupdate && CERTH_Trackr.ctrack.getCurrentPosition() && null!=CERTH_Trackr.canvas && CERTH_Trackr.ctrack.draw(CERTH_Trackr.canvas)
		
		// retrieve tracker current parameters
		var cp = CERTH_Trackr.ctrack.getCurrentParameters();	
		this.emotionResults = CERTH_Trackr.emotionClassifier.meanPredict(cp);
		
		// convert to Valence/Arousal Space
		return this.dominantPointDistance();
	},

	dominantPointDistance: function() {
		var tempPoint = [ 0, 0 ];
        
		if(CERTH_Trackr.emotionResults) {
			// Anger, Disgust, Fear, Happy, Sad, Surprise
			var valance = [ 2.34, 2.45, 2.76, 1.61, 7.47, 8.21 ];
			var arousal = [ 7.63, 5.42, 6.96, 4.13, 7.47, 6.49 ];

			//Find Max
			var maxValue = -9999;
			var maxIndex = -1;
			
			for(var i = 0; i < CERTH_Trackr.emotionResults.length; i++) {
				if(CERTH_Trackr.emotionResults[i].value > maxValue) {
					maxValue = CERTH_Trackr.emotionResults[i].value;
					maxIndex = i;
				}
			}

			if(maxIndex >= 0) {
				tempPoint[0] += CERTH_Trackr.emotionResults[maxIndex].value * valance[maxIndex];
				tempPoint[1] += CERTH_Trackr.emotionResults[maxIndex].value * arousal[maxIndex];
			}
		}

		CERTH_Trackr.currentVA = tempPoint;
        return tempPoint;
    }, 
	
	// streaming feature points
	stream:function(filename){
		if(CERTH_Trackr.needsupdate){
			if(CERTH_Trackr.emotionResults && CERTH_Trackr.currentVA){
				// create string to write to file
				var r= '@' + CERTH_Trackr.uuid_user + '@' + CERTH_Trackr.uuid_game + "@" + CERTH_Trackr.currentVA[0] + '@' + CERTH_Trackr.currentVA[1];
				r+="\n",
				
				// emit timestamped features
				CERTH_Trackr.socket.emit("tracker_shape_features",new Date().toISOString().concat(r))
			}
		}
	}
};
