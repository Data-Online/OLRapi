angular.module('app', ['autofields'])
    .controller('registrationCtrl', ['$scope', '$log', 'registrationDataSource', function ($scope, $log, registrationDataSource) {
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

        $scope.fieldTrips = {
            fieldTrip1: { choices: {} }
        };

        $scope.fieldTripsSchema = [
            {
                type: 'multiple', fields: [
                    { property: 'fieldTrip1.choices[0]', type: 'select', list: 'value as value for (key,value) in fieldTrip1Options', attr: { required: true } },
                    { property: 'fieldTrip1.choices[1]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip1Options,1)', attr: { required: true } },
                    { property: 'fieldTrip1.choices[2]', type: 'select', list: 'value as value for (key,value) in excludeItems1(fieldTrip1Options,2)', attr: { required: true } }
                ], columns: 6
            }
        ];

        $scope.excludeItems1 = function (itemList, index) {
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
            $log.info($scope.user);
            testWrite($scope.user);

            alert('You\'ve joined!\n\nSee console for additional info.');
        }

        $log.info("TEST");

        var testRead = function () {
            //registrationDataSource.getRegistrationTemplate()
            registrationDataSource.getRegistrationData()
                .then(bindData, onError);
        };

        var testWrite = function (data) {
            registrationDataSource.saveRegistrationDetails(data);
        }

        var bindData = function (data) {
            $log.info("Data;");
            $log.info(data);
            $log.info(data.email);
            $scope.user = data;
            $scope.fieldTrips = data.fieldTrip1;
            $scope.displayEmail = data.email;
            $scope.fieldTrip1Options = data.fieldTrip1.options;
            //$scope.items = data.fieldTrip1Options.options;
            $log.info($scope.fieldTrip1Options);

        }

        var onError = function () {
            $log.error("Error");
        }

        $scope.addField = function () {
            testRead();
        };

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
