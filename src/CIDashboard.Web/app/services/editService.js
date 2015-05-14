(function () {
    'use strict';
    angular.module('services').factory('editService',  ['$signalrService', '$rootScope', function (signalrService, $rootScope) {

        var initialize = function () {
            $rootScope.editMode = false;

            //Getting the connection object
            signalrService.initialize();

            setTimeout(requesProjectBuilds, 200);

            signalrService.getProxy().on('refreshStatus', function (status) {
                $rootScope.$broadcast('refreshStatus', status);
            });

            signalrService.getProxy().on('stopRefresh', function (status) {
                $rootScope.$broadcast('stopRefresh', status);
            });

            signalrService.getProxy().on('sendProjectBuilds', function (projectBuilds) {
                $rootScope.$broadcast('sendProjectBuilds', projectBuilds);
            }); 
        };

        var requestRefresh = function () {
            signalrService.getProxy()
                .invoke('RequestRefresh')
                .fail(function (error) {
                console.log('RequestRefresh error: ' + error);
            });
        };

        var requesProjectBuilds = function () {
            if (!signalrService.isConnected())
                setTimeout(requesProjectBuilds, 200);

            signalrService.getProxy()
                .invoke('RequestAllProjectBuilds')
                .fail(function (error) {
                console.log('RequestAllProjectBuilds error: ' + error);
            });
        };

        //add, update projects
        var addNewProject = function (project) {
            signalrService.getProxy()
                .invoke('AddNewProject', project)
                .fail(function (error) {
                    console.log('AddNewProject error: ' + error);
                });
        };

        var updateProjectName = function (projectId, projectName) {
            signalrService.getProxy()
                .invoke('updateProjectName', projectId, projectName)
                .fail(function (error) {
                    console.log('updateProjectName error: ' + error);
                });
        };

        var removeProject = function (projectId) {
            signalrService.getProxy()
                .invoke('removeProject', projectId)
                .fail(function (error) {
                    console.log('removeProject error: ' + error);
                });
        }


        //add, update build results
        var addBuildToProject = function (projectId, build) {
            signalrService.getProxy()
                .invoke('addBuildToProject', projectId, build)
                .fail(function (error) {
                    console.log('addBuildToProject error: ' + error);
                });
        };
    
        var removeBuild = function (buildId) {
            signalrService.getProxy()
                .invoke('removeBuild', buildId)
                .fail(function (error) {
                    console.log('removeBuild error: ' + error);
                });
        }
    
        var updateBuildNameAndExternalId = function (buildId, buildName, ciExternalId) {
            signalrService.getProxy()
                .invoke('updateBuildConfigExternalId', buildId, buildName, ciExternalId)
                .fail(function (error) {
                    console.log('updateBuildConfigExternalId error: ' + error);
                });
        }



        return {
            initialize: initialize,
            requestRefresh: requestRefresh,
            requesProjectBuilds: requesProjectBuilds,
            addNewProject: addNewProject,
            updateProjectName: updateProjectName,
            removeProject: removeProject,
            addBuildToProject: addBuildToProject,
            removeBuild: removeBuild,
            updateBuildNameAndExternalId: updateBuildNameAndExternalId
        };
    }]);
}());