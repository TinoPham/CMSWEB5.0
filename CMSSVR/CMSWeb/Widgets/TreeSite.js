(function ($) {
	'use strict';

	$(document).ready(function () {

		var defaultData = [
          {
          	text: 'I3 International',
          	href: '#parent1',
          	tags: ['200'],
          	nodes: [
              {
              	text: 'Headquarters',
              	href: '#child1',
              	tags: ['16'],
              	nodes: [
                  {
                  	text: 'Building Toronto 1',
                  	href: '#grandchild1',
                  	tags: ['8']
                  },
                  {
                  	text: 'Building Toronto 2',
                  	href: '#grandchild2',
                  	tags: ['6']
                  },
				  {
				  	text: 'Building Toronto 3',
				  	href: '#grandchild2',
				  	tags: ['4']
				  },
				   {
				   	text: 'Building Toronto 4',
				   	href: '#grandchild2',
				   	tags: ['12']
				   }
              	]
              },//end Headquarters
			  {
			  	text: 'North Branch',
			  	href: '#child1',
			  	tags: ['32'],
			  	nodes: [
                  {
                  	text: 'Building Toronto 1',
                  	href: '#grandchild1',
                  	tags: ['8']
                  },
                  {
                  	text: 'Building Toronto 2',
                  	href: '#grandchild2',
                  	tags: ['6']
                  },
				  {
				  	text: 'Building Toronto 3',
				  	href: '#grandchild2',
				  	tags: ['4']
				  },
				   {
				   	text: 'Building Toronto 4',
				   	href: '#grandchild2',
				   	tags: ['12']
				   }
			  	]
			  },//end North Branch
			  {
			  	text: 'South West Branch',
			  	href: '#child1',
			  	tags: ['48'],
			  	nodes: [
                  {
                  	text: 'Building Toronto 1',
                  	href: '#grandchild1',
                  	tags: ['8']
                  },
                  {
                  	text: 'Building Toronto 2',
                  	href: '#grandchild2',
                  	tags: ['6']
                  },
				  {
				  	text: 'Building Toronto 3',
				  	href: '#grandchild2',
				  	tags: ['4']
				  },
				   {
				   	text: 'Building Toronto 4',
				   	href: '#grandchild2',
				   	tags: ['12']
				   }
			  	]
			  },//end South West Branch
			  {
			  	text: 'West Branch',
			  	href: '#child1',
			  	tags: ['28'],
			  	nodes: [
                  {
                  	text: 'Building Toronto 1',
                  	href: '#grandchild1',
                  	tags: ['8']
                  },
                  {
                  	text: 'Building Toronto 2',
                  	href: '#grandchild2',
                  	tags: ['6']
                  },
				  {
				  	text: 'Building Toronto 3',
				  	href: '#grandchild2',
				  	tags: ['4']
				  },
				   {
				   	text: 'Building Toronto 4',
				   	href: '#grandchild2',
				   	tags: ['12']
				   }
			  	]
			  },//end West Branch
			  {
			  	text: 'East Branch',
			  	href: '#child1',
			  	tags: ['48'],
			  	nodes: [
                  {
                  	text: 'Building Toronto 1',
                  	href: '#grandchild1',
                  	tags: ['8']
                  },
                  {
                  	text: 'Building Toronto 2',
                  	href: '#grandchild2',
                  	tags: ['6']
                  },
				  {
				  	text: 'Building Toronto 3',
				  	href: '#grandchild2',
				  	tags: ['4']
				  },
				   {
				   	text: 'Building Toronto 4',
				   	href: '#grandchild2',
				   	tags: ['12']
				   }
			  	]
			  }
          	]
          }
		];

		//this is used right now
		$('#treeview-tags').treeview({
			color: '#428bca',
			expandIcon: 'glyphicon glyphicon-chevron-right',//'icon-node-collapse', //'glyphicon glyphicon-play',
			collapseIcon: 'glyphicon glyphicon-chevron-down',//'icon-node-expande',
			nodeIcon: 'glyphicon glyphicon-folder-open',
			showTags: true,
			data: defaultData,
			showBorder: false,
			onNodeSelected: function (event, node) {
				//alert('dd');
			}
		});
	});

}(jQuery));