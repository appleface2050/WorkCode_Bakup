app.controller('LayoutController', function ($scope, $location, $window,ApiFactory,$q,$timeout,$rootScope) {


  $scope.session_id = Number(new Date());
    var modes = {
        portrait_1: "layout_1",
        portrait_2: "layout_2",
        portrait_3: "layout_3",
        portrait_4: "layout_4",
        landscape_1: "layout_1",
        landscape_2: "layout_2",
        landscape_3: "layout_3",
        landscape_4: "layout_4"
    };

    var layouts = {
        layout_1: {
            isPortrait: null,
            name: "layout_1",
            portrait: {
                BlueStacksWebcam: {
                    enableWebCam: true,
                    x: 0,
                    y: 0,
                    width: 100,
                    height: 100,
                    actualWidth: 0,
                    actualHeight: -1
                },
                BlueStacks: {
                    x: 62,
                    y: 32,
                    width: 38,
                    height: 68
                }
            },
            landscape: {
                BlueStacksWebcam: {
                    enableWebCam: true,
                    x: 0,
                    y: 0,
                    width: 100,
                    height: 100,
                    actualWidth: 0,
                    actualHeight: -1
                },
                BlueStacks: {
                    x: 32,
                    y: 62,
                    width: 68,
                    height: 38
                }
            },
            order: "watermarkGif,watermark,BlueStacks,BlueStacksWebcam,CLR Browser",
            logo: "none"
        },
        layout_2: {
            isPortrait: null,
            name: "layout_2",
            portrait: {
                BlueStacksWebcam: {
                    enableWebCam: true,
                    x: 0,
                    y: 0,
                    width: 43,
                    height: 100,
                    actualWidth: 0,
                    actualHeight: -1
                },
                BlueStacks: {
                    x: 43,
                    y: 0,
                    width: 57,
                    height: 100
                }
            },
            landscape: {
                BlueStacksWebcam: {
                    enableWebCam: true,
                    x: 0,
                    y: 57,
                    width: 100,
                    height: 43,
                    actualWidth: 0,
                    actualHeight: -1
                },
                BlueStacks: {
                    x: 0,
                    y: 0,
                    width: 100,
                    height: 57
                }
            },
            order: "watermarkGif,watermark,BlueStacks,BlueStacksWebcam,CLR Browser",
            logo: "none"
        },
        layout_3: {
            isPortrait: null,
            name: "layout_3",
            portrait: {
                BlueStacks: {
                    x: 22,
                    y: 0,
                    width: 56,
                    height: 100
                },
                BlueStacksWebcam: 
                {
                    enableWebCam: false
                }
           },
           landscape: {
                BlueStacks: {
                    x: 0,
                    y: 22,
                    width: 100,
                    height: 56
                },
                BlueStacksWebcam: {
                    enableWebCam: false
                }
           },
           order: "watermarkGif,watermark,BlueStacks,BlueStacksWebcam,CLR Browser",
           logo: "none"
        },
        layout_4: {
            isPortrait: null,
            name: "layout_4",
            portrait: {
                BlueStacksWebcam: {
                    enableWebCam: true,
                    x: 0,
                    y: 0,
                    width: 100,
                    height: 100,
                    actualWidth: 0,
                    actualHeight: -1
                }
            },
            landscape: {
                BlueStacksWebcam: {
                    enableWebCam: true,
                    x: 0,
                    y: 0,
                    width: 100,
                    height: 100,
                    actualWidth: 0,
                    actualHeight: -1
                }
            },
            order: "watermarkGif,watermark,BlueStacks,BlueStacksWebcam,CLR Browser",
            logo: "none"
        }
    };

    $scope.session_id = Number(new Date());

    $scope.select_mode = function (mode) {
	var isAppView = false;
	if (mode == "portrait_3" || mode == "landscape_3")
		isAppView = true;
	
        var modeJson = JSON.stringify({"layoutTheme": layouts[modes[mode]], "window": "LayoutWindow", "isAppView": isAppView});
        console.log('calling gmapi with modejson');
        console.log(modeJson);
        GmApi.gmDialogClickHandler(modeJson);
        $scope.activeMode = mode;
        ApiFactory
           .Edit_filter_stats('layout_selected', null, $scope.session_id, $scope.activeMode,$scope.live)
           .then(function (data) {
               console.log('data:');
               console.log(data);
           });
    };

    $scope.closeFilterWindow = function () {
        console.log('closing filter window');
        ApiFactory
            .Edit_filter_stats('cancelled_edit_layout','Clicked_on_x_button',$scope.session_id,$scope.activeMode,$scope.live);
        GmApi.gmCloseDialog({type: "close",stats: []});
    };

    $scope.filter_done_Clicked =function(){
        ApiFactory
        .Edit_filter_stats('layout_applied',null,$scope.session_id,$scope.activeMode,$scope.live);
        GmApi.gmCloseDialog({type: "done",stats: []});
    };


});
