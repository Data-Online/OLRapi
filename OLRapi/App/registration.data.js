(function () {
    var registrationDataSource = function ($http) {


        var getRegistrationData = function (userGuid) {
            var dataApi = "/api/registration/" + userGuid;
            return $http.get(dataApi)
                .then(function (response) {
                    return response.data;
                });
        };

        var saveRegistrationDetails = function (registration, userGuid) {
            var dataApi = "/api/saveRegistrationDetails/" + userGuid;
            var config = {
                headers: {
                    'Content-Type': 'application/json'
                }
            }
            return $http.post(dataApi, JSON.stringify(registration), config)
                .then(function (response) {
                    return response; //.data;
                });
        };

        var getForeignKeyData = function (adminAccess) {
            var dataApi = "/api/getForeignKeyData/" + adminAccess;
            return $http.get(dataApi)
                .then(function (response) {
                    return response.data;
                });
        };

        var getCurrentCost = function (userGuid) {
            var dataApi = "/api/getCurrentCost/" + userGuid;
            return $http.get(dataApi)
                .then(function (response) {
                    return response.data;
                });
        };

        var sendRegistration = function (userGuid) {
            var targetApi = "/api/sendRegistration/" + userGuid;
            console.log("Target : " + targetApi);
            return $http.get(targetApi)
                .then(function (response) {
                    console.log("Status: " + response.status);
                    return response.status;
                });
        };

        return {
            getRegistrationData: getRegistrationData,
            saveRegistrationDetails: saveRegistrationDetails,
            getForeignKeyData: getForeignKeyData,
            getCurrentCost: getCurrentCost,
            sendRegistration: sendRegistration
        }

    };
    var module = angular.module("app");
    module.factory("registrationDataSource", registrationDataSource);

}());