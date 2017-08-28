angular.module('app', ['autofields', 'multipleSelect', 'toaster'])
    .controller('registrationCtrl', ['$scope', '$log', '$location', 'registrationDataSource', 'toaster', '$anchorScroll', function ($scope, $log, $location, registrationDataSource, toaster, $anchorScroll) {
        $scope.loading = false;
        $scope.validRegistration = false;
        $scope.currentCosts = [];

        var registrationUid = $location.search().Registration;
        $scope.linkActive = $location.search().A == "dps";
        var showCosts = false;
        

        //$log.info("RW : " + overrideSaveBlock + " : " + $location.search().A);

        $scope.registration = {
            userDetails: {
                firstName: '',
                lastName: '',
                homeTown: '',
                email: '',
                PSNZMember: false,
                NZIPPMember: false,
                PSNZMemberAppliedFor: false,
                photoHonours: []
            },
            fieldTrips: {
                choices: {}
            }
        };

        $scope.userSchema = [
            {
                type: 'multiple', fields: [
                    { property: 'userDetails.firstName', label: 'First Name', type: 'text', attr: { required: true } },
                    { property: 'userDetails.lastName', label: 'Last Name', type: 'text', attr: { required: true } }
                ], columns: 6
            },
            {
                type: 'multiple', fields: [
                    { property: 'userDetails.homeTown', label: 'Home Town', type: 'select', list: 'value as value for (key,value) in towns', attr: { required: true } },
                    { property: 'userDetails.mobileNumber', label: 'Mobile Number', type: 'text', attr: { required: false } }
                ], columns: 6

            },
            {
                type: 'multiple', fields: [
                    {
                        property: 'userDetails.PSNZMember', label: 'PSNZ Member', type: 'checkbox',
                        attr: { ngChange: 'registration.userDetails.PSNZMemberAppliedFor = false' }
                    },
                    {
                        property: 'userDetails.PSNZMemberAppliedFor', label: 'PSNZ Membership applied for', type: 'checkbox',
                        attr: { ngChange: 'registration.userDetails.PSNZMember = false' }
                    },
                    { property: 'userDetails.NZIPPMember', label: 'NZIPP Member', type: 'checkbox' }
                ], columns: 6
            }
        ];

        $scope.registrationSchema = [
            {
                type: 'multiple', fields: [
                    {
                        property: 'registrationDetails.additionalDinnerTicket',
                        label: 'Additional Dinner Ticked Neeeded?', type: 'checkbox',
                        attr: { required: false, ngShow: 'selectedRegistration == "Full convention including awards dinner"' }
                    },
                    {
                        property: 'registrationDetails.additionalDinnerName', label: 'Additional Name', type: 'text',
                        attr: { required: false, ngShow: '$data.registrationDetails.additionalDinnerTicket && selectedRegistration == "Full convention including awards dinner"' }
                    }],
                columns: 4
            },
            {
                property: 'registrationDetails.specialRequirements', label: 'Special requirements and additional information',
                type: 'textarea', rows: 5, placeholder: 'Also please enter your honours or hometown if they were not in the list above, or anything else you think we need to know.',
                attr: { required: false }
            }
        ];

        $scope.fieldTrip0Schema = [
            {
                type: 'multiple', fields: [
                    {
                        property: 'fieldTrips[0].choices[0]', label: 'First Choice', type: 'select', list: 'value as value for (key,value) in fieldTrip0Options',
                        attr: { required: true }
                    },
                    {
                        property: 'fieldTrips[0].choices[1]', label: 'Second Choice', type: 'select', list: 'value as value for (key,value) in excludeItems(fieldTrip0Options,1,0)',
                        attr: { required: true }
                    },
                    {
                        property: 'fieldTrips[0].choices[2]', label: 'Third Choice', type: 'select', list: 'value as value for (key,value) in excludeItems(fieldTrip0Options,2,0)',
                        attr: { required: true }
                    }
                ], columns: 4
            }
        ];

        $scope.fieldTrip1Schema = [
            {
                type: 'multiple', fields: [
                    {
                        property: 'fieldTrips[1].choices[0]', label: 'First Choice', type: 'select', list: 'value as value for (key,value) in fieldTrip1Options',
                        attr: { required: true }
                    },
                    {
                        property: 'fieldTrips[1].choices[1]', label: 'Second Choice', type: 'select', list: 'value as value for (key,value) in excludeItems(fieldTrip1Options,1,1)',
                        attr: { required: true }
                    },
                    {
                        property: 'fieldTrips[1].choices[2]', label: 'Third Choice', type: 'select', list: 'value as value for (key,value) in excludeItems(fieldTrip1Options,2,1)',
                        attr: { required: true }
                    }
                ], columns: 4
            }
        ];

        $scope.workshopsSchema = [
            { property: 'registrationDetails.canonWorkshop', label: 'Register for the Canon Workshop', type: 'checkbox' }
        ]

        $scope.excludeItems = function (itemList, index, fieldTripIndex) {
            //$log.info('Called');
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
                showMessages: true
            },
            layout: {
                type: 'basic',
                labelSize: 3,
                inputSize: 9
            }
        };

        //$scope.genders = {
        //    0: 'Male',
        //    1: 'Female'
        //};

        //$scope.genderCheck = {
        //    0: 'No',
        //    1: 'Yes'
        //};


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

        $scope.register = function () {
            if (!$scope.registerForm.$valid) return;
            //join stuff....
            // $log.info($scope.registration);
            //$log.info("UID : " + registrationUid);
            saveRegistrationData($scope.registration);
            //alert('You\'ve joined!\n\nSee console for additional info.');
        }

        var sendRegistrationEmail = function (userGuid) {
            //$log.info("Send details ...");
            registrationDataSource.sendRegistration(userGuid)
                .then(notifyEmail, onError);
        };

        var notifyEmail = function () {
            toaster.pop('success', "Confirmation", "A confirmation eMail has been sent.");
           // alert('Email confirmation sent!');
        };

        var readRegistrationDetails = function (userGuid) {
            $scope.loading = true;
            registrationDataSource.getRegistrationData(userGuid)
                .then(bindData, onError);
        };

        //var testRead = function () {
        //    //registrationDataSource.getRegistrationTemplate()
        //    registrationDataSource.getRegistrationData()
        //        .then(bindData, onError);
        //};

        var saveRegistrationData = function (data) {
            data.userDetails.photoHonours = $scope.selectedHonours;
            data.userDetails.photoClubs = $scope.selectedClubs;
            data.registrationDetails.registrationType = $scope.selectedRegistration
            registrationDataSource.saveRegistrationDetails(data, registrationUid)
                .then(savedOkay, onError);
        };

        var savedOkay = function () {
            //alert('Thank you!\n\nYour registration has been saved. An email confirmation will follow.\n\nScroll to the top of this page to review the total cost due.');
            toaster.pop('success', "Thank you!", "Your registration has been saved. An email confirmation will follow.");
            $anchorScroll();
//            toaster.pop('success', "Total Cost", "Scroll to the top of this page to review the total cost due.");

            if (showCosts) {
                getCosts();
            };

            sendRegistrationEmail(registrationUid);
        };

        $scope.sendRegistrationAgain = function () {
            sendRegistrationEmail(registrationUid);
        };

        var getFKData = function () {
            registrationDataSource.getForeignKeyData()
                .then(bindFKData, onError);
        }

        var bindData = function (data) {
            $scope.validRegistration = true;

            //$log.info("Data;");
            //$log.info(data);
            //$log.info(data.email);
            $scope.registration = data;
            $scope.displayEmail = data.userDetails.email;
            // $scope.towns = data.towns;

            // Registration details
            $scope.selectedRegistration = data.registrationDetails.registrationType;

            // Field trip 0
            $scope.fieldTrip0Description = data.fieldTrips[0].fieldTripDescription;
            $scope.fieldTrip0Options = data.fieldTrips[0].options;
            // Field trip 1
            $scope.fieldTrip1Description = data.fieldTrips[1].fieldTripDescription;
            $scope.fieldTrip1Options = data.fieldTrips[1].options;

            $scope.selectedHonours = data.userDetails.photoHonours;
            $scope.selectedClubs = data.userDetails.photoClubs;

            if (!$scope.linkActive) 
            {
                $scope.linkActive = (data.registrationDetails.linkExpiryDate < Date());
            }
            //$scope.items = data.fieldTrip1Options.options;
            //$log.info($scope.fieldTrip1Options);
            $scope.loading = false;

            getFKData();

            if (showCosts) {
                getCosts();
            };

        }

        var bindFKData = function (data) {
            $scope.towns = data.towns;
            // $scope.photoHonours = data.photoHonours;
            $scope.honoursList = data.photoHonours;
            $scope.clubsList = data.photoClubs;
            $scope.registrationTypes = data.registrationTypes;
        }

        var onError = function () {
            $scope.loading = false;
            $scope.validRegistration = false;
            $log.error("Error");
        }

        readRegistrationDetails(registrationUid);

        var getCosts = function () {
            registrationDataSource.getCurrentCost(registrationUid)
                .then(bindCostsData, onError);
            //$log.info("Cost: " + zz);
            //7853C644-A356-4B87-A26F-DC15FBD2F415
            //readRegistrationDetails("7853C644-A356-4B87-A26F-DC15FBD2F415");  //7853C644-A356-4B87-A26F-DC15FBD2F415
        }

        var bindCostsData = function (data) {
            $log.info("Get Total : ");
            $scope.currentCosts = data;
            $scope.currentCostsTitle = "Registration Total";
            $scope.getTotal = function () {
                var total = 0.00;
                for (var i = 0; i < $scope.currentCosts.length; i++) {
                    total += $scope.currentCosts[i].Cost;
                    $log.info("Total : " + total);
                }
                return total;
            }
        }

    }])
    .config(function ($locationProvider) {
        $locationProvider.html5Mode(
            {
                enabled: true,
                requireBase: false
            });
    })
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
