angular.module('app', ['autofields'])
    .controller('registrationCtrl', ['$scope', '$log', 'registrationDataSource', function ($scope, $log, registrationDataSource) {
        $scope.loading = false;
        $scope.validRegistration = false;

        $scope.user = {
            firstName: '',
            lastName: '',
            PSNZMember: false,
            NZIPPMember: false,
            PSNZMemberAppliedFor: false,
            fieldTrip1Choice1: null,
            fieldTrip1Choice2: null,
            fieldTrip1Choice3: null
            //,
            //fieldTrip1: { choices: {} }
//            fieldTrips: null
        };


        $scope.schema = [
            {
                type: 'multiple', fields: [
                    { property: 'firstName', type: 'text', attr: { required: true } },
                    { property: 'lastName', type: 'text', attr: { required: true } }
                ], columns: 6
            },
            {
                type: 'multiple', fields: [
                    { property: 'PSNZMember', label: 'PSNZ Member', type: 'checkbox' },
                    { property: 'NZIPPMember', label: 'NZIPP Member', type: 'checkbox' },
                    { property: 'PSNZMemberAppliedFor', label: 'PSNZ Membership applied for', type: 'checkbox' }
                ], columns: 6
            }
            //,
            //{
            //    type: 'multiple', fields: [
            //        { property: 'fieldTrip1.choices[0]', type: 'select', list: 'value as value for (key,value) in fieldTrip1Options', attr: { required: true } },
            //        { property: 'fieldTrip1.choices[1]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip1Options,1)', attr: { required: true } },
            //        { property: 'fieldTrip1.choices[2]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip1Options,2)', attr: { required: true } }
            //    ], columns: 6
            //}

        ];

        $scope.registration = {
            firstName: '',
            lastName: '',
            PSNZMember: false,
            NZIPPMember: false,
            PSNZMemberAppliedFor: false,
            fieldTrip1Choice1: null,
            fieldTrip1Choice2: null,
            fieldTrip1Choice3: null,

            fieldTrips: {
                choices: {}
            }
        };

        $scope.userSchema = [
            {
                type: 'multiple', fields: [
                    { property: 'firstName', type: 'text', attr: { required: true } },
                    { property: 'lastName', type: 'text', attr: { required: true } }
                ], columns: 6
            },
            {
                type: 'multiple', fields: [
                    { property: 'PSNZMember', label: 'PSNZ Member', type: 'checkbox' },
                    { property: 'NZIPPMember', label: 'NZIPP Member', type: 'checkbox' },
                    { property: 'PSNZMemberAppliedFor', label: 'PSNZ Membership applied for', type: 'checkbox' }
                ], columns: 6
            }
        ];


        $scope.fieldTrip0Schema = [
            {
                type: 'multiple', fields: [
                    { property: 'fieldTrips[0].choices[0]', type: 'select', list: 'value as value for (key,value) in fieldTrip0Options', attr: { required: true } },
                    { property: 'fieldTrips[0].choices[1]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip0Options,1,0)', attr: { required: true } },
                    { property: 'fieldTrips[0].choices[2]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip0Options,2,0)', attr: { required: true } }
                ], columns: 6
            }
        ];

        $scope.fieldTrip1Schema = [
            {
                type: 'multiple', fields: [
                    { property: 'fieldTrips[1].choices[0]', type: 'select', list: 'value as value for (key,value) in fieldTrip1Options', attr: { required: true } },
                    { property: 'fieldTrips[1].choices[1]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip1Options,1,1)', attr: { required: true } },
                    { property: 'fieldTrips[1].choices[2]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip1Options,2,1)', attr: { required: true } }
                ], columns: 6
            }
        ];

        $scope.excludeItems1z = function (itemList, index) {
            $log.info('Called');
            var result = {};
            var addToList = false;
            angular.forEach(itemList, function (value, key) {
                if (!angular.isUndefinedOrNull($scope.user.fieldTrip1.choices)) {
                    if (index == 2)
                    {
                        addToList = !angular.equals(value, $scope.user.fieldTrip1.choices[0]) & !angular.equals(value, $scope.user.fieldTrip1.choices[1])
                    }
                    else {
                        addToList = !angular.equals(value, $scope.user.fieldTrip1.choices[0]);
                    }
                    if (addToList) {
                        result[key] = value;
                    }
                }
            });

            return result;
        };

        $scope.excludeItems1 = function (itemList, index, fieldTripIndex) {
            $log.info('Called');
            var result = {};
            var addToList = false;
            angular.forEach(itemList, function (value, key) {
                if (!angular.isUndefinedOrNull($scope.registration.fieldTrips[fieldTripIndex].choices)) {
                    if (index == 2) {
                        addToList = !angular.equals(value, $scope.registration.fieldTrips[fieldTripIndex].choices[0]) & !angular.equals(value, $scope.registration.fieldTrips[fieldTripIndex].choices[1])
                    }
                    else {
                        addToList = !angular.equals(value, $scope.registration.fieldTrips[fieldTripIndex].choices[0]);
                    }
                    if (addToList) {
                        result[key] = value;
                    }
                }
            });

            return result;
        };


        angular.isUndefinedOrNull = function (val) {
            return angular.isUndefined(val) || val === null
        }

        $scope.options = {
            validation: {
                enabled: true,
                showMessages: false
            },
            layout: {
                type: 'basic',
                labelSize: 3,
                inputSize: 9
            }
        };

        $scope.genders = {
            0: 'Male',
            1: 'Female'
        };

        $scope.genderCheck = {
            0: 'No',
            1: 'Yes'
        };

        $scope.toggleValidation = function () {
            $scope.options.validation.enabled = !$scope.options.validation.enabled;
        };

        $scope.togglePopovers = function () {
            $scope.options.validation.showMessages = !$scope.options.validation.showMessages;
        };

        $scope.toggleHorizontal = function () {
            $scope.options.layout.type = $scope.options.layout.type == 'horizontal' ? 'basic' : 'horizontal';
        }

        ////$scope.addField = function () {
        ////    $scope.schema.push({ property: 'new' + (new Date().getTime()), label: 'New Field' });
        ////};

        $scope.join = function () {
            if (!$scope.joinForm.$valid) return;
            //join stuff....
            $log.info($scope.registration);
            testWrite($scope.registration);

            alert('You\'ve joined!\n\nSee console for additional info.');
        }

        var readRegistrationDetails = function (userGuid) {
            $scope.loading = true;
            registrationDataSource.getRegistrationData(userGuid)
                .then(bindData, onError);
        };

        var testRead = function () {
            //registrationDataSource.getRegistrationTemplate()
            registrationDataSource.getRegistrationData()
                .then(bindData, onError);
        };

        var testWrite = function (data) {
            registrationDataSource.saveRegistrationDetails(data);
        }

        var bindData = function (data) {
            $scope.validRegistration = true;

            $log.info("Data;");
            $log.info(data);
            $log.info(data.email);
            $scope.registration = data;
            $scope.displayEmail = data.userDetails.email;


            // Field trip 0
            $scope.fieldTrip0Description = data.fieldTrips[0].fieldTripDescription;
            $scope.fieldTrip0Options = data.fieldTrips[0].options;
            // Field trip 1
            $scope.fieldTrip1Description = data.fieldTrips[1].fieldTripDescription;
            $scope.fieldTrip1Options = data.fieldTrips[1].options;


            //$scope.items = data.fieldTrip1Options.options;
            $log.info($scope.fieldTrip1Options);
            $scope.loading = false;

        }

        var onError = function () {
            $scope.loading = false;
            $scope.validRegistration = false;
            $log.error("Error");
        }

        $scope.addField = function () {
            testRead();
        };

        readRegistrationDetails("96BE1CE0-27F4-4A46-BDCD-EBAB16A1EA27");

    }])
    .directive('confirmPassword', [function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                var validate = function (viewValue) {
                    var password = scope.$eval(attrs.confirmPassword);
                    ngModel.$setValidity('match', ngModel.$isEmpty(viewValue) || viewValue == password);
                    return viewValue;
                }
                ngModel.$parsers.push(validate);
                scope.$watch(attrs.confirmPassword, function (value) {
                    validate(ngModel.$viewValue);
                })
            }
        }
    }]);
