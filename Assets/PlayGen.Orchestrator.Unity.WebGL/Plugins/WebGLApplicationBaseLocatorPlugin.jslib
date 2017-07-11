// implement an external method to return the web application root
// this depends on the function in PlayGen.UnitProxy, but must be a jslib plugin or there is no way to return a value synchronously

var WebGLWebServiceLocatorPlugin = { 
	GetApplicationBase: function () {
		var debug = true;
		debug && console.debug("GetApplicationBase");

		var path = window.PlayGen.UnityProxy.GetApplicationBase();

		var buffer = _malloc(lengthBytesUTF8(path) + 1);
			
		debug && console.debug("HttpRequest::writeStringToMemory");
		writeStringToMemory(path, buffer);
		return buffer;
	}
};

mergeInto(LibraryManager.library, WebGLWebServiceLocatorPlugin);
