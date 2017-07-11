var EmoCC = function(config, verbose) {

  if (typeof verbose=="undefined")
    verbose = false;

  var self = this; // like python

  // different browser have different APIs
  window.AudioContext = window.AudioContext || window.webkitAudioContext;
  navigator.getUserMedia = ( navigator.getUserMedia ||
                         navigator.webkitGetUserMedia ||
                         navigator.mozGetUserMedia ||
                         navigator.msGetUserMedia);

  if (typeof config.callback=="undefined" || config.callback==null)
    this.audiocallback =  null;
  else
    this.audiocallback = config.callback;

  // create an audio connection
  this.input_source = null;
  this.sample_size = 2048; // only power of 2 sizes supported
  this.num_channels = 1;
  try {
    this.audio_context = new AudioContext();
    verbose && console.log("audio context: ok");
    verbose && console.log("getUserMedia: "+(navigator.getUserMedia?'ok':'fail'));
    verbose && console.log("audio context sample rate: "+this.audio_context.sampleRate)
    this.audio_ok = true;
  }
  catch (e) {
    console.log("ERROR: no audio support");
    this.audio_ok = false;
  }

  // connect to the websocket
  this.isopen = false;
  try {
    if (typeof config.url=="undefined" || config.url==null)
      throw "no websocket endpoint defined";
    this.socket = new WebSocket(config.url);
    this.socket.binaryType = "arraybuffer";

    this.socket.onopen = function(e) {
      verbose && console.log("WS opened");
      self.isopen = true;
    }

    this.socket.onclose = function(e) {
      verbose && console.log("WS closed: "+e);
      self.socket = null;
      self.isopen = false;
    }

  }
  catch(err) {
    console.log("ERROR: cannot open websocket: "+err);
  }

  // audio operations

  this.toPCM = function(input, offset, output) {
    for (var i=0; i<input.length; i++, offset+=2) {
      var sample = input[i];
      var samppcm = sample*0x8000;
      samppcm = Math.max(samppcm, -32768);
      samppcm = Math.min(samppcm, 32767);
      output.setInt16(offset, samppcm, true);
    }
  }

  this.downsample = function(buffer, in_srate, out_srate) {
    // trivial case
    if (in_srate==out_srate)
      return buffer;
    // make sure we are downsampling
    if (out_srate>in_srate)
      throw "output rate is bigger than input";
    var ratio = in_srate/out_srate;
    var newlength = Math.round(buffer.length/ratio);
    var result = new Float32Array(newlength);
    var ri = 0;
    var bi = 0;
    while (ri<result.length) {
      var next_bi = Math.round((ri+1)*ratio);
      var accum = 0;
      var count = 0;
      // linear interpolation
      for (var i=bi; i<next_bi && i<buffer.length; i++) {
        accum += buffer[i];
        count++;
      }
      result[ri] = Math.min(1, accum/count);
      ri++;
      bi = next_bi;
    }

    return result;
  }

  this.hasMicrophone = function() {
    return self.audio_ok;
  }


  this.startMicrophone = function(stream) {
    self.input_source = self.audio_context.createMediaStreamSource(stream);
    self.node = self.audio_context.createScriptProcessor(self.sample_size, self.num_channels, self.num_channels);
    self.input_source.connect(self.node);
    self.node.connect(self.audio_context.destination);

    verbose && console.log('input stream number of inputs: '+self.input_source.numberOfInputs);
    verbose && console.log('input stream number of outputs: '+self.input_source.numberOfOutputs);
    verbose && console.log('input stream number of channels: '+self.input_source.channelCount);
    verbose && console.log('input stream channel type: '+self.input_source.channelInterpretation);
    verbose && console.log('node number of inputs: '+self.node.numberOfInputs);
    verbose && console.log('node number of outputs: '+self.node.numberOfOutputs);
    verbose && console.log('node number of channels: '+self.node.channelCount);
    verbose && console.log('node channel type: '+self.node.channelInterpretation);
  }

  this.startRecording = function() {
    if (self.audio_ok) {
      verbose && console.log("start recording");
      self.node.onaudioprocess = function (e) {
        verbose && console.log("received frame "+e.playbackTime+" "+e.inputBuffer.length);
        self.audiocallback(e);
      }
    }
  }

  this.stopRecording = function () {
    if (self.audio_ok) {
      verbose && console.log("stop recording");
      self.node.onaudioprocess = null;
    }
  }

  // communications

  this.createStartMessage = function(username, gamename) {
    var data = new ArrayBuffer(91);
    var dv = new DataView(data);
    dv.setUint8(0, 1);
    dv.setFloat64(1, window.performance.now(), true); // always little endian
    for (var si=0, pi=9; si<username.length; si++, pi++) {
      dv.setUint8(pi, username.charCodeAt(si) % 0x7f, true);
    }
    for (var si=0, pi=45; si<gamename.length; si++, pi++) {
      dv.setUint8(pi, gamename.charCodeAt(si) % 0x7f, true);
    }
    dv.setUint16(81, 8, true);
    verbose && console.log(window.performance.timing.navigationStart);
    dv.setFloat64(83, window.performance.timing.navigationStart, true);

    return data;

  }

  this.createStopMessage = function(username, gamename) {
    var data = new ArrayBuffer(83);
    var dv = new DataView(data);
    dv.setUint8(0, 2);
    dv.setFloat64(1, window.performance.now(), true); // little endian
    for (var si=0, pi=9; si<username.length; si++, pi++) {
      dv.setUint8(pi, username.charCodeAt(si) % 0x7f, true);
    }
    for (var si=0, pi=45; si<gamename.length; si++, pi++) {
      dv.setUint8(pi, gamename.charCodeAt(si) % 0x7f, true);
    }
    dv.setUint16(81, 0, true);

    return data;

  }

  this.createAudioMessage = function(username, gamename, audio, srate) {
    if (srate!=16000)
      audio = this.downsample(audio, srate, 16000);

    var data = new ArrayBuffer(83+audio.length*2);
    var dv = new DataView(data);
    dv.setUint8(0, 4);
    dv.setFloat64(1, window.performance.now(), true); // little endian
    for (var si=0, pi=9; si<username.length; si++, pi++) {
      dv.setUint16(pi, username.charCodeAt(si) % 0x7f, true);
    }
    for (var si=0, pi=45; si<gamename.length; si++, pi++) {
      dv.setUint16(pi, gamename.charCodeAt(si) % 0x7f, true);
    }
    dv.setUint16(81, audio.length*2, true);
    this.toPCM(audio, 83, dv);

    return data;
  }

  this.sendAudio = function(username, gamename, data, srate) {
    verbose && console.log("sendAudio: "+self.isopen);
    if (self.isopen) {
      if (typeof srate=="undefined" || srate==null)
        srate = self.audio_context.sampleRate;
      var message = this.createAudioMessage(username, gamename, data, srate);
      self.socket.send(message);
      verbose && console.log("audio data sent :"+data.length);
    }
    else {
      console.log("ERROR: connection not open");
    }
  }

  this.sendStart = function(username, gamename) {
    if (self.isopen) {
      var message = this.createStartMessage(username, gamename);
      self.socket.send(message);
      verbose && console.log("start sent");
    }
    else {
      console.log("ERROR: connection not open");
    }
  }

  this.sendStop = function(username, gamename) {
    if (self.isopen) {
      var message = this.createStopMessage(username, gamename);
      self.socket.send(message);
      verbose && console.log("stop sent");
    }
    else {
      console.log("ERROR: connection not open");
    }
  }

  // initialise the microphone
  navigator.getUserMedia({audio: true}, this.startMicrophone, function(e) {
    console.log("ERROR: no audio input : "+e);
    this.audio_ok = false;
  });

} // emocc
