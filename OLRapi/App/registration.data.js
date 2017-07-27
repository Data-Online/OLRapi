(function () {
    var registrationDataSource = function ($http) {


        var getRegistrationData = function (userGuid) {
            var dataApi = "/api/registration/" + userGuid;
            return $http.get(dataApi)
                .then(function (response) {
                    return response.data;
                });
        };

        var saveRegistrationDetails = function (registration) {
            var dataApi = "/api/saveRegistrationDetails";
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

        return {
            getRegistrationData: getRegistrationData,
            saveRegistrationDetails: saveRegistrationDetails
        }

    };
    var module = angular.module("app");
    module.factory("registrationDataSource", registrationDataSource);

}());