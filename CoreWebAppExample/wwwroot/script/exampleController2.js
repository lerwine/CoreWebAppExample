(function () {
    'use strict';

    angular
        .module('exampleApp')
        .controller('exampleController2', controller);

    controller.$inject = ['$location']; 

    function controller($location) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'controller';

        activate();

        function activate() { }
    }
})();
