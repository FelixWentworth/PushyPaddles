/*
/////////////////////////////////////////////////////////////////////////
//
// (c) University of Southampton IT Innovation Centre, 2016
//
// Copyright in this software belongs to University of Southampton
// IT Innovation Centre of Gamma House, Enterprise Road,
// Chilworth Science Park, Southampton, SO16 7NS, UK.
//
// This software may not be used, sold, licensed, transferred, copied
// or reproduced in whole or in part in any manner or form or in or
// on any media by any person other than in accordance with the terms
// of the Licence Agreement supplied with the software, or otherwise
// without the prior written consent of the copyright owners.
//
// This software is distributed WITHOUT ANY WARRANTY, without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
// PURPOSE, except where stated in the Licence Agreement supplied with
// the software.
//
//      Created By :            Simon Crowle
//      Created Date :          17-10-2016
//      Created for Project :   ProsocialLearn
//
/////////////////////////////////////////////////////////////////////////
*/

/**
 * PSLClient class used to interoperate with the PsL platform
 */
function PSLClient()
{
    var libBasePath = "";
    var libsRemaingingToLoad = 0;

    var userFTConfig = null;
    var userVTConfig = null;
    var userInitCB   = null;

    var timeSyncClient = null;

    var faceTrackConfig = null;
    var faceEmotionCB = null;
    var faceTrackerInitOK = false;
    var trackingFace = false;

    var voiceSender = null;
    var voiceTrackingCB = null;
    var voiceTrackerInitOK = false;
    var trackingVoice = false;

    var sampleRateMS = 200;

    var registryService =
        {
            ep : "https://sta-psl.atosresearch.eu/sr/PsLDB/PsL-Ext-Services/",
            un : "admin",
            pw : "changeit"
        };

    // Default configuration to be re-written by registry information
    var apiConfiguration =
        {
            playerNickName     : undefined,
            playerID           : undefined,
            gameInstanceID     : undefined,
            lastTimeSyncOffset : 0,

            timeSyncEP     : undefined,
            faceTrackerEP  : undefined,
            voiceTrackerEP : undefined
        };

    // Public methods ----------------------------------------------------------

    /**
     * Use this method to initialise voice and face emotion tracking.
     * vtc - Voice tracking initialisation parameters (can be null)
     * ftc - Face tracking initialisation parameters (can be null)
     * rCB  - Call indicating the initialisation result
     *
     * Throws if config params are both null or cannot get platform
     * configuration information.
     *
     */
    this.initialise = function( vtc, ftc, rCB )
    {
        console.log( "Initialising PSL Client" );

        if ( vtc === undefined || ftc === undefined )
            throw "Please define or make null both voice and face configurations";

        userVTConfig = vtc;
        userFTConfig = ftc;

        if ( rCB !== undefined )
            userInitCB = rCB;

        getPsLLibPath();

        // Determine number of scripts to loaded
        libsRemaingingToLoad = 4 + ( userVTConfig !== null ? 1 : 0 ) + ( userFTConfig !== null ? 8 : 0 );

        // Load base scripts
        addScriptRef( "https://code.jquery.com/jquery-3.1.1.min.js" );

        // Time sync Services
        addScriptRef( libBasePath + "thirdParty/timesync.js" );
        addScriptRef( "https://cdnjs.cloudflare.com/ajax/libs/es5-shim/4.0.5/es5-shim.min.js" );
        addScriptRef( "https://cdnjs.cloudflare.com/ajax/libs/es6-shim/0.23.0/es6-shim.min.js" );

        // Voicer scripts, if required
        if ( userVTConfig !== null )
            loadVoicerScripts();

        // Face tracking scripts, if requried
        if ( userFTConfig !== null )
            loadFaceTrackingScripts();
    }

    /**
     * Use this method to shutdown the client
     */
    this.shutdown = function()
    {
        this.trackVoice( false );
        this.trackFace( false );

        faceTrackConfig = null;
        faceEmotionCB = null;
        faceTrackerInitOK = false;
        trackingFace = false;

        voiceSender = null;
        voiceTrackingCB = null;
        voiceTrackerInitOK = false;
        trackingVoice = false;

        timeSyncClient.on( 'change', undefined );
        timeSyncClient = null;
    }

    /**
     * Use this method to retrieve information about the current game player
     * (if made available by the PSL platform)
     *
     * Returns { playerNickName  : nick name of player (string),
     *           playerID        : unique identifier of player (UUID)
     *           gameInstanceID  : unique identifier of game instance (UUID)
     *           localTimeOffset : offset (in milliseconds) of your local clock relative to the PsL server
     *  }
     */
    this.getGameInfo = function ()
    {
        return { playerNickName  : apiConfiguration.playerNickName,
                 playerID        : apiConfiguration.playerID,
                 gameInstanceID  : apiConfiguration.gameInstanceID,
                 localTimeOffset : apiConfiguration.lastTimeSyncOffset };
    }

    /**
     * Use this method to start tracking the user's face for emotions. You must
     * first have initialised the client with a valid face tracking configuration.
     *
     * trackingOn - Use 'true' to start tracking, 'false' to stop tracking
     *
     */
    this.trackFace = function( trackingOn )
    {
        if ( faceTrackerInitOK === true && trackingOn !== undefined )
        {
            if ( trackingOn === true )
            {
                CERTH_Trackr.start();
                trackingFace = true;

                updateFaceTracking();
            }
            else
            {
                CERTH_Trackr.stop();
                trackingFace = false;
            }
        }
        else
            console.log( "Could not track face: not initialised or invalid params" );
    }

    /**
     * Use this method to start tracking the user's voice for emotions. You must
     * first have initialised the client with a valid voice tracking configuration.
     *
     * trackingOn - Use 'true' to start tracking, 'false' to stop tracking
     */
    this.trackVoice = function( trackingOn )
    {
        if ( voiceTrackerInitOK === true && trackingOn !== undefined )
        {
            if ( trackingOn === true )
            {
                try
                {
                    voiceSender.startRecording();

                    voiceSender.sendStart( apiConfiguration.playerID,
                                           apiConfiguration.gameInstanceID );
                                           
                    trackingVoice = true;
                }
                catch ( ex )
                {
                    console.log( "Had problems starting voice tracking: " + ex );
                    trackingVoice = false;
                }
            }
            else
            {
                try
                {
                    voiceSender.stopRecording();
                    voiceSender.sendStop();
                    trackingVoice = false;
                }
                catch ( ex )
                {
                    console.log( "Had problems stopping voice tracking: "+  ex );
                }
            }
        }
    }

    // Private methods --------------------------------------------------------
    function getPsLLibPath()
    {
        var scripts = document.getElementsByTagName("script");
        var sl = Object.keys(scripts).length;

        for ( var i = 0; i < sl; i++ )
        {
            var s = scripts[i];
            if ( s.src != null )
            {
                var sInd = s.src.indexOf("PSLClientAPI.js");
                if ( sInd > 0 )
                {
                    libBasePath = s.src.substring(0, sInd);
                    break;
                }
            }
        }
    }

    function tryPullUserInfoFromPlatform()
    {
        // First check for cookie data
        var pslCookieDict = extractCookieData();

        if ( pslCookieDict['nick'] !== undefined && pslCookieDict['nick'] !== '' &&
             pslCookieDict['studentID'] !== undefined && pslCookieDict['studentID'] !== '' &&
             pslCookieDict['gameID'] !== undefined && pslCookieDict['gameID'] !== '' )
        {
            apiConfiguration.playerNickName = pslCookieDict['nick'];
            apiConfiguration.playerID = pslCookieDict['studentID'];
            apiConfiguration.gameInstanceID = pslCookieDict['gameID'];
        }
        else
        {
            var err = "Failed to initialise: Could not find PSL user information";
            console.log( err );

            if ( userInitCB !== null )
                userInitCB( err );
        }
    }

    function extractCookieData()
    {
        var pslData = [];

        var cTokens = document.cookie.split(';');

        for ( var i = 0; i < cTokens.length; i++ )
        {
            var cTV = cTokens[i].split('=');
            var k = ( cTV[0] !== undefined ) ? cTV[0].trim() : undefined;
            var v = ( cTV[1] !== undefined ) ? cTV[1].trim() : undefined;

            if ( k !== undefined && v !== undefined )
                pslData[k] = v;
        }

        return pslData;
    }

    function attemptSetupWithPlatform()
    {
        // Start pulling data from platform
        try
        {
            tryPullUserInfoFromPlatform();
            tryPullPlatformServiceEndpoints();
        }
        catch ( ex )
        {
            if ( userInitCB !== null )
                userInitCB( 'Failed to initialise: ' + ex );
        }
    }

    function tryPullPlatformServiceEndpoints()
    {
        // Do this without using jQuery as we cannot assume it will be loaded
        var epXHR = new XMLHttpRequest();

        epXHR.onreadystatechange = function()
        {
            if ( this.readyState == 4 && this.status == 200 )
                onPlatformServiceEndpointsOK( this.responseText );

            else if ( this.readyState == 4 && this.status !== 200 )
                onPlatformServiceEndpointsFailed( this.statusText );
        }

        epXHR.open( "GET", registryService.ep, true );

        var unpw = btoa( registryService.un + ":" + registryService.pw );
        epXHR.setRequestHeader( "Authorization", "Basic " + unpw );

        epXHR.send();
    }

    function onPlatformServiceEndpointsOK( result )
    {
        try
        {
            var json = JSON.parse( result );
            var rDoc = json._embedded;
            var endPoints = [];

            for ( var i = 0; i < Object.keys(rDoc).length; i++ )
                endPoints[ rDoc[i]._id ] = rDoc[i].URL;

            apiConfiguration.timeSyncEP = endPoints["tss"];
            apiConfiguration.voiceTrackerEP = endPoints[ "EMOCS" ];
            apiConfiguration.faceTrackerEP = endPoints[ "FTS" ];

            onPlatformConfigFoundOK();
        }
        catch ( ex )
        {
            if ( userInitCB !== null )
                userInitCB( 'Failed to initialise: ' + ex );
        }
    }

    function onPlatformServiceEndpointsFailed( statusText )
    {
        console.log( "Failed to initialise: PSL end-points unavailable" );

        if ( userInitCB !== null )
            userInitCB( 'Failed to initialise: ' + statusText );
    }

    function onPlatformConfigFoundOK()
    {
        var finalInitResult = "OK";

        // Initialise time synchronization, if available
        if ( apiConfiguration.timeSyncEP !== undefined )
        {
            timeSyncClient = timesync.create({ server: apiConfiguration.timeSyncEP, interval: 10000 });
            timeSyncClient.on('change', onTimeSyncOffset );
        }
        else
            finalInitResult = "Could not get valid time synchronisation end-point";

        // Initialise voice tracking, if required and available
        if ( userVTConfig !== null )
        {
            if ( apiConfiguration.voiceTrackerEP !== undefined )
                initaliseVoiceTracker( userVTConfig );
            else
                finalInitResult = "Could not get valid voice tracking end-point";
        }

        // Initialise face tracking, if required
        if ( userFTConfig !== null )
        {
            if ( apiConfiguration.faceTrackerEP !== undefined )
                initialiseFaceTracker( userFTConfig );
            else
                finalInitResult = "Could not get valid face tracking end-point";
        }

        // Finally notify user of outcome
        if ( finalInitResult === "OK" )
        {
            console.log( "PSL Client initialised OK" );

            if ( userInitCB !== null )
                userInitCB( finalInitResult );
        }
        else
        {
            console.log( "PSL Client initialisation failed: " + finalInitResult );

            if ( userInitCB !== null )
                userInitCB( finalInitResult );
        }
    }

    function initialiseFaceTracker( ftc )
    {
        if ( faceTrackerInitOK === false )
        {
            var videoIN = document.getElementById( ftc.videoID );
            var faceCanvas = document.getElementById( ftc.canvasID );
            var faceOutContext = faceCanvas.getContext( '2d' );
            faceEmotionCB = ftc.trackingCallback;

            try
            {
                CERTH_Trackr.init( videoIN, ftc.width, ftc.height,
                                   faceCanvas, faceOutContext,
                                   apiConfiguration.playerID,
                                   apiConfiguration.gameInstanceID );

                var ftSock = io.connect( apiConfiguration.faceTrackerEP );
                CERTH_Trackr.socket = ftSock;

                faceTrackerInitOK = true;

                console.log( "Initialised face tracking OK" );
            }
            catch ( ex )
            {
                // Unfortunately expections are always raised during initialisation
                console.log( "Caught face tracking initalisation error: " + ex );
            }
        }
    }

    function initaliseVoiceTracker( vtc )
    {
        if ( voiceTrackerInitOK === false )
        {
            voiceTrackingCB = vtc.trackingCallback;

            try
            {
                vconfig = {url: apiConfiguration.voiceTrackerEP, callback: onVoiceAudioSample};
                voiceSender = new EmoCC(vconfig);
                voiceTrackerInitOK = true;

                console.log( "Initialised voice tracking OK" );
            }
            catch ( ex )
            {
                console.log( "Caught voice tracking initialisation error: " + ex );
                voiceTrackerInitOK = false;
            }
        }
    }

    function updateFaceTracking()
    {
        if ( trackingFace )
        {
            try
            {
                var er = CERTH_Trackr.track();

                if ( er !== null )
                {
                    // Send on to server here
                    CERTH_Trackr.stream();

                    // Callback locally, if required
                    if ( faceEmotionCB !== null )
                        faceEmotionCB( er );
                }
            }
            catch ( err )
            { console.log( "Caught face tracking error"); }

            // Perform another track & classification at the sample rate
            window.setTimeout( function() { requestAnimationFrame( updateFaceTracking ); }, sampleRateMS );
        }
    }

    function onVoiceAudioSample( evt, verbose )
    {
        if ( evt !== undefined && evt !== null )
        {
            var channelData = evt.inputBuffer.getChannelData(0);

            voiceSender.sendAudio( apiConfiguration.playerID,
                                   apiConfiguration.gameInstanceID,
                                   channelData );

            if ( voiceTrackingCB !== null )
                voiceTrackingCB( channelData );
        }
    }

    function loadVoicerScripts()
    {
        addScriptRef( libBasePath + 'emocc/emocc.js' );
    }

    function loadFaceTrackingScripts()
    {
        addScriptRef( libBasePath + 'thirdParty/clmtrackr.min.js' );
        addScriptRef( libBasePath + 'thirdParty/jsfeat-min.js' );
        addScriptRef( libBasePath + 'thirdParty/numeric.js' );
        addScriptRef( libBasePath + 'thirdParty/socket.io-1.4.5.js' );

        addScriptRef( libBasePath + 'face_tracking/prosociallearn-facetracking.min.js' );
        addScriptRef( libBasePath + 'face_tracking/CERTH_Trackr.js' );

        addScriptRef( libBasePath + 'face_tracking/emotion_classifier.js' );
        addScriptRef( libBasePath + 'face_tracking/emotion_model.js' );
    }

    function addScriptRef( scriptPath )
    {
        if ( scriptPath !== undefined )
        {
            var scr = document.createElement( 'script' );
            scr.type = "text/javascript";
            scr.src = scriptPath;
            scr.addEventListener( "load", onScriptLoaded, false );

            document.body.appendChild( scr );
        }
    }

    function onScriptLoaded()
    {
        libsRemaingingToLoad--;

        // If we're done, continue with initialisation
        if ( libsRemaingingToLoad == 0 )
            attemptSetupWithPlatform();
    }

    function onTimeSyncOffset( offset )
    {
        if ( offset !== undefined )
            apiConfiguration.lastTimeSyncOffset = offset;
    }
}

// Test functions; not for production use
// ----------------------------------------------------------------------------
function __clearTestData()
{
    document.cookie = "nick=";
    document.cookie = "studentID=";
    document.cookie = "gameID=";

    console.log( "Cleared cookie data" );
}

function __createTestUserCookie()
{
    document.cookie = "nick=test player";
    document.cookie = "studentID=00000000-0000-0000-0000-000000000000";
    document.cookie = "gameID=00000000-0000-0000-0000-000000000000";
}
