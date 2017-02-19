﻿var viewerApp;
var model;

function showModel(urn) {
    var options = {
        env: 'AutodeskProduction',
        getAccessToken: getAccessToken,
        refreshToken: getAccessToken
    };

    var documentId = 'urn:' + urn;

    Autodesk.Viewing.Initializer(options, function onInitialized() {

        viewerApp = new Autodesk.Viewing.ViewingApplication('MyViewerDiv');

        //Configure the extension
        var config3D = {
            extensions: ["ForgeViewerExtension"]
        };

        viewerApp.registerViewer(viewerApp.k3D, Autodesk.Viewing.Private.GuiViewer3D, config3D);

        viewerApp.loadDocument(documentId, onDocumentLoadSuccess, onDocumentLoadFailure);
    });
}

/**
* Autodesk.Viewing.ViewingApplication.loadDocument() success callback.
* Proceeds with model initialization.
*/
function onDocumentLoadSuccess(doc) {

    // We could still make use of Document.getSubItemsWithProperties()
    // However, when using a ViewingApplication, we have access to the **bubble** attribute,
    // which references the root node of a graph that wraps each object from the Manifest JSON.
    var viewables = viewerApp.bubble.search({ 'type': 'geometry' });
    if (viewables.length === 0) {
        console.error('Document contains no viewables.');
        return;
    }

    document = doc;

    // Choose any of the avialble viewables
    viewerApp.selectItem(viewables[0].data, onItemLoadSuccess, onItemLoadFail);
   
}

/**
 * Autodesk.Viewing.ViewingApplication.loadDocument() failure callback. 
 * @param {} viewerErrorCode 
 * @returns {} 
 */
function onDocumentLoadFailure(viewerErrorCode) {
    console.error('onDocumentLoadFailure() - errorCode:' + viewerErrorCode);
}

/**
 * viewerApp.selectItem() success callback.
 * @param {} viewer 
 * @param {} item 
 * @returns {} 
 */
function onItemLoadSuccess(viewer, item) {
    console.log('onItemLoadSuccess()!');
    console.log(viewer);
    console.log(item);

    // Congratulations! The viewer is now ready to be used.
    console.log('Viewers are equal: ' + (viewer === viewerApp.getCurrentViewer()));

    model = viewer.model;

}

/**
 * viewerApp.selectItem() failure callback.
 */
function onItemLoadFail(errorCode) {
    console.error('onItemLoadFail() - errorCode:' + errorCode);
}

/**
* the JavaScript getAccessToken on client-side. 
* To retrive viewer token
*/
function getAccessToken() {
    var xmlHttp = null;
    xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", '/api/forge/token/', false /*forge viewer requires SYNC*/);
    xmlHttp.send(null);
    return xmlHttp.responseText;
}


/**
 * Create an extension
 */
function ForgeViewerExtension(viewer, options) {
    Autodesk.Viewing.Extension.call(this, viewer, options);
}

ForgeViewerExtension.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
ForgeViewerExtension.prototype.constructor = ForgeViewerExtension;

/**
 * Event triggered when selecting an object
 */
ForgeViewerExtension.prototype.onSelectionEvent = function (event) {
    var currSelection = this.viewer.getSelection();
    var domElem = document.getElementById('MySelectionValue');
    domElem.innerText = currSelection.length;

    var prop = model.getProperties(currSelection[0], getPropertiesSuccess, getPropertiesFailure);
    //var properties = currSelection[0]
};

/**
 * Triggered when I found properties for a selection item
 */
function getPropertiesSuccess(parameters) {
    var domElem = document.getElementById('MyPropertiesSelected');
    domElem.innerText = parameters;
    var props = parameters.properties;

    for (var i = 0; i < props.length ; i++) {
        
    }
}

/**
 * Triggered when I found properties for a selection item
 */
function getPropertiesFailure(parameters) {
    var domElem = document.getElementById('MyPropertiesSelected');
    domElem.innerText = "failure";
}

/**
 * Triggered when the extension ForgeViewerExtension is loaded
 * @returns {} 
 */
ForgeViewerExtension.prototype.load = function () {

    var viewer = this.viewer;

    //Load the selection event
    this.onSelectionBinded = this.onSelectionEvent.bind(this);
    this.viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, this.onSelectionBinded);

    var lockBtn = document.getElementById('MyAwesomeLockButton');
    lockBtn.addEventListener('click', function () {
        viewer.setNavigationLock(true);
    });

    var unlockBtn = document.getElementById('MyAwesomeUnlockButton');
    unlockBtn.addEventListener('click', function () {
        viewer.setNavigationLock(false);
    });

    return true;
};

/**
 * Triggered when the extension ForgeViewerExtension is unloaded
 * @returns {} 
 */
ForgeViewerExtension.prototype.unload = function () {
    alert('ForgeViewerExtension is now unloaded!');

    //Unload the selection  event
    this.viewer.removeEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, this.onSelectionBinded);
    this.onSelectionBinded = null;
    return true;
};

Autodesk.Viewing.theExtensionManager.registerExtension('ForgeViewerExtension', ForgeViewerExtension);


