(function(factory) {
    "use strict";

    if (typeof define === "function" && define.amd) {
        define(factory);
    } else if (typeof module != "undefined" && typeof module.exports != "undefined") {
        module.exports = factory();
    } else if (typeof Package !== "undefined") {
        CMSTree = factory();
    } else {
        window["CMSTree"] = factory();
    }
})(function() {

    function CMSTree(el, def, options) {

        var placeholder = null;
        var dragged;
        var over;
        var dragData;
        var dragDataNode;
        var dropData;
        var selectedNode = {};
        var selectedScope;
        var element;
        var allowDrop = false;
        var autoselect = false;

        var defOriginal = {
            Id: 'Id',
            Name: 'Name',
            Type: 'Type',
            Childs: 'Sites',
            Checked: 'Checked',
            Count: 'SiteCount',
            Model: {}
        }

        var defCol = {}

        var opNode = {
            Node: {
                IsShowIcon: true,
                IsShowCheckBox: true,
                IsShowRootNode: true,
                IsShowNodeMenu: true,
                IsShowCount: false,
                IsShowAddNodeButton: true,
                IsShowAddItemButton: true,
                IsShowEditButton: true,
                IsShowDelButton: true,
                IsDraggable: true,
                IsDraggableFile: true,
                InitNode: true,
                IsCollapsedAll: null
            },
            Item: {
                IsShowItemMenu: true,
                IsShowDelButton: true,
                IsShowEditButton: true,
                IsShowIcon: true,
                IsAllowFilter: true,
                IsRadio: false,
                IsHightlightFilter: true
            },
            Icon: {
                Expand: 'icon-minus-squared',
                Collpased: 'icon-plus-squared',
                NodeExpand: 'icon-map-pointer2',
                GroupExpand: 'icon-home-1',
                NodeCollpased: 'icon-pin56',
                GroupCollpased: 'icon-store-3',
                RootIcon: 'icon-company-2',
                NodeAdd: 'icon-plus',
                NodeAddItem: 'icon-plus-circled-1',
                NodeEdit: 'icon-pencil',
                NodeDel: 'icon-cancel-2',
                Item: 'icon-store-3',
                ItemDel: 'icon-cancel-2',
                ItemEdit: 'icon-pencil'
            },
            CallBack: {
                AddNode: null,
                EditNode: null,
                DelNode: null,
                EditItem: null,
                DelItem: null,
                selectedNode: selectedNode,
                SelectedFn: null,
                CheckNodeFn: null,
                DblClickNode: null,
                filterText: "",
                DragStart: null,
                DragOver: null,
                SetIconGroup: null,
                SetIconFolder: null,
                SetIconFile: null,
                DragEnd: null
            }

        };

        var settings = {}

        var NType = {
            Folder: 0,
            Group: 1,
            File: 2
        }

        var NodeType = {}

        var TreeNodeContent = React.createClass({
            displayName: "TreeNodeContent",
            checkFn: function(chck, scope) {
                checkNodeStatus(this.props.node, this.refs.checkbox);
                this.props.refresh(chck);
                if (settings.CallBack.CheckNodeFn) {
                    settings.CallBack.CheckNodeFn(this.props.node, this);
                }
            },
            toogleExpand: function() {
                this.props.collapsed = !this.props.collapsed;
                this.props.toggleCollapsed(this.props.collapsed);
            },
            selectedNodeFn: function(e) {
                settings.CallBack.selectedNode = this.props.node;

                if (selectedScope) {
                    selectedScope.forceUpdate();
                }

                if (settings.CallBack.SelectedFn) {
                    settings.CallBack.SelectedFn(this.props.node, this);
                }
                selectedScope = this;
                this.forceUpdate();
                if (this.props.forceUp) {
                    this.props.forceUp();
                }
            },
            selectedFn: function(node, scope) {
                settings.CallBack.selectedNode = node;

                if (selectedScope) {
                    selectedScope.forceUpdate();
                }
                selectedScope = scope;

                this.forceUpdate();
                if (this.props.forceUp) {
                    this.props.forceUp();
                }
            },
            dblClickNodeFn: function() {
                settings.CallBack.selectedNode = this.props.node;

                if (selectedScope) {
                    selectedScope.forceUpdate();
                }

                if (settings.CallBack.DblClickNode) {
                    settings.CallBack.DblClickNode(this.props.node, this);
                }
                selectedScope = this;
                this.forceUpdate();
            },
            addNode: function(scope) {
                AddNode(this);
            },
            addNodeItem: function(scope) {
                AddNode(this);
            },
            editNode: function(scope) {
                EditNode(this);
            },
            delNode: function(scope) {
                DelNode(this);
            },
            refreshNodeFn: function() {
                this.props.forceUp();
            },
            render: function() {
                var node = this.props.node;
                var classSelect, addRegion, addSite, editRegionSite, deleteRegionSite, sitecount, boxcontrol, checkbox;
                var collapsed = this.props.collapsed;

                if (settings.CallBack.selectedNode && settings.CallBack.selectedNode[defCol.Id] === this.props.node[defCol.Id] && settings.CallBack.selectedNode[defCol.Type] === this.props.node[defCol.Type]) {
                    classSelect = 'select';
                }

                var classCollapsed = collapsed === true ? settings.Icon.Collpased : settings.Icon.Expand;

                var classText;
                if (this.props.isRootNode && this.props.isRootNode === true) {
                    classText = settings.Icon.RootIcon;
                } else {
                    classText = collapsed === true ? this.props.Icon.Collapsed : this.props.Icon.Expand;
                }
                classText = settings.Node.IsShowIcon === true ? classText : '';

                var textConent = (
                    React.createElement("a", { className: "btn btn-xs", onClick: this.selectedNodeFn.bind(this), onDoubleClick: this.dblClickNodeFn.bind(this) },
                        React.createElement("i", { className: classText }),
                        React.createElement(ContentHightLight, { node: this.props.node, filterText: this.props.filterText })
                    )
                );

                sitecount = settings.Node.IsShowCount === true ? (React.createElement("b", { className: "badge pull-right" }, node[defCol.Count])) : sitecount;

                editRegionSite = settings.Node.IsShowEditButton === true ? (React.createElement("a", { className: "btn btn-xs edit", onClick: this.editNode.bind(this) }, React.createElement("i", { className: settings.Icon.NodeEdit }))) : editRegionSite;
                addRegion = settings.Node.IsShowAddNodeButton === true ? (React.createElement("a", { className: "btn btn-xs add", onClick: this.addNode.bind(this) }, " ", React.createElement("i", { className: settings.Icon.NodeAdd }))) : addRegion;
                addSite = settings.Node.IsShowAddItemButton === true ? (React.createElement("a", { className: "btn btn-xs add", onClick: this.addNodeItem.bind(this) }, React.createElement("i", { className: settings.Icon.NodeAddItem }))) : addSite;

                if (settings.Node.IsShowDelButton === true) {
                    deleteRegionSite = (React.createElement("a", { className: "btn btn-xs del", onClick: this.delNode.bind(this) }, React.createElement("i", { className: settings.Icon.NodeDel })));
                }

                var labelcheckbox, checkboxclass = "node-content i-checks";;
                if (settings.Node.IsShowCheckBox === true) {
                    checkboxclass += ' cmscheckbox';
                    var checkid = 'item-' + this._reactInternalInstance._rootNodeID + node[defCol.Type] + '.' + node[defCol.Id];
                    var checkstats = this.props.node[defCol.Checked] === true ? true : false;
                    checkbox = (React.createElement("input", { type: "checkbox", id: checkid, checked: checkstats, ref: "checkbox", onClick: this.checkFn.bind(this, node) }));
                    if (this.props.node[defCol.Checked] === null) {
                        labelcheckbox = (React.createElement("label", { htmlFor: checkid }, React.createElement("span", null)));
                    } else {
                        labelcheckbox = (React.createElement("label", { htmlFor: checkid }, React.createElement("i", { className: " icon-ok-1" })));
                    }

                }

                boxcontrol = settings.Node.IsShowNodeMenu === true ? (
                    React.createElement("b", { className: "control-group pull-right" },
                        addRegion,
                        addSite,
                        editRegionSite,
                        deleteRegionSite
                    )
                ) : boxcontrol;

                return (React.createElement("div", { className: classSelect },
                    React.createElement("div", { className: "tree-node-content" },
                        React.createElement("a", { className: "btn btn-xs", onClick: this.toogleExpand }, React.createElement("i", { className: classCollapsed })),
                        React.createElement("span", { className: checkboxclass },
                            checkbox,
                            labelcheckbox,
                            textConent,
                            sitecount,
                            boxcontrol
                        )
                    )
                ));
            }
        });

        var TreeNode = React.createClass({
            displayName: "TreeNode",
            //checkFn: function(chck, scope){
            //   this.props.check(chck);
            //},
            refreshFn: function(chck, scope) {
                this.props.refresh(chck);
            },
            dragStart: function(node, e) {
                dragStartFn(this, e, node)
            },
            dragEnd: function(e) {
                dragEndFn(this, e);
            },
            dragOver: function(e) {
                dragOverFn(this, e);
            },
            forceUp: function() {
                this.props.forceUp();
            },
            render: function() {
                var node = this.props.node;
                var collapsed = this.props.collapsed;

                var collapsedClass = '';
                if (collapsed === true) {
                    collapsedClass = 'hidden';
                }

                var treenode = [];
                if (node[defCol.Childs] && node[defCol.Childs].length > 0) {

                    //treenode = node[defCol.Childs].map(function(n,i){
                    //  if(settings.Item.IsAllowFilter === true){
                    //        var reg = RegExp('(' + this.props.filterText + ')', 'gi');
                    //        if (n[defCol.Type] === 1 &&  n[defCol.Name].search(reg) === -1) {
                    //          return;
                    //        }
                    //     }
                    //     return (<li data-id={i} key={i} draggable="true" data-drag-handle="true" onDragEnd={this.dragEnd} onDragStart={this.dragStart}><Tree node = {n} filterText = {this.props.filterText} check={this.checkFn} /></li>);
                    //}.bind(this));


                    node[defCol.Childs].forEach(function(n, i) {
                        //if (settings.Item.IsAllowFilter === true) {
                        //    var reg = new RegExp(this.props.filterText.replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1"), "i");
                        //    //var reg = RegExp('(' + this.props.filterText + ')', 'gi');
                        //    if (n[defCol.Type] === NodeType.File && n[defCol.Name] && n[defCol.Name].search(reg) === -1) {
                        //        return;
                        //    }
                        //}

                        if (n.isShow !== undefined && n.isShow === false) return;

                        if (settings.Node.IsDraggable === true || (n[defCol.Type] === NodeType.File && settings.Node.IsDraggableFile === true)) {
                            treenode.push(React.createElement("li", { "data-id": i, "data-type-id": n[defCol.Type], key: n[defCol.Type] + '.' + n[defCol.Id], draggable: "true", "data-drag-handle": "true", onDragEnd: this.dragEnd, onDragStart: this.dragStart.bind(this, n) }, React.createElement(Tree, { node: n, parentNode: node, filterText: this.props.filterText, forceUp: this.props.forceUp.bind(this), refresh: this.refreshFn })));
                        } else {
                            treenode.push(React.createElement("li", { "data-id": i, "data-type-id": n[defCol.Type], key: n[defCol.Type] + '.' + n[defCol.Id] }, React.createElement(Tree, { node: n, parentNode: node, filterText: this.props.filterText, forceUp: this.props.forceUp.bind(this), refresh: this.refreshFn })));
                        }

                    }.bind(this));

                    if (settings.Node.IsDraggable === true || (node[defCol.Type] === NodeType.File && settings.Node.IsDraggableFile === true)) {
                        return (React.createElement("ol", { className: collapsedClass, "data-type-id": node[defCol.Type], "data-id": node[defCol.Id], onDragOver: this.dragOver }, treenode));
                    } else {
                        return (React.createElement("ol", { className: collapsedClass, "data-type-id": node[defCol.Type], "data-id": node[defCol.Id] }, treenode));
                    }
                } else {
                    if (settings.Node.IsDraggable === true || (node[defCol.Type] === NodeType.File && settings.Node.IsDraggableFile === true)) {
                        var treenode = (React.createElement("li", { className: "node-hidden", "data-type-id": node[defCol.Type], "data-id": -1, "data-drag-handle": "true" }));
                        return (React.createElement("ol", { className: collapsedClass, "data-type-id": node[defCol.Type], "data-id": node[defCol.Id], onDragOver: this.dragOver }, treenode));
                    } else {
                        var treenode = (React.createElement("li", { className: "node-hidden", "data-type-id": node[defCol.Type], "data-id": -1 }));
                        return (React.createElement("ol", { className: collapsedClass, "data-type-id": node[defCol.Type], "data-id": node[defCol.Id] }, treenode));
                    }
                }
            }
        });

        var ContentHightLight = React.createClass({
            displayName: "ContentHightLight",
            render: function() {
                var filterT = this.props.filterText;
                var text = this.props.node[defCol.Name];
                if (text && filterT && filterT !== "" && settings.Item.IsHightlightFilter === true) {
                    var reg = filterT.replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1");
                    text = text.replace(new RegExp('(' + reg + ')', 'gi'), '<span class="highlighted">$1</span>');
                    //text = text.replace(new RegExp("(" + filterT + ")", "gi"), '<span class="highlighted">$1</span>');
                    return (React.createElement("span", { className: "node-name", title: text, dangerouslySetInnerHTML: { __html: text } }));
                }

                return (React.createElement("span", { className: "node-name", title: text }, text));
            }
        });

        var FileNodeContent = React.createClass({
            displayName: "FileNodeContent",
            checkFn: function(chck, parentNode, scope) {
                if (settings.Item.IsRadio === true) {
                    parentNode[defCol.Childs].forEach(function(n) {
                        if (n[defCol.Id] !== chck[defCol.Id]) {
                            n[defCol.Checked] = false;
                        } else {
                            n[defCol.Checked] = n[defCol.Checked] === true ? false : true;
                            //chck[defCol.Checked] = chck[defCol.Checked] === true ? false : true;
                        }
                    });
                } else {
                    this.props.node[defCol.Checked] = this.props.node[defCol.Checked] === true ? false : true;
                }
                React.findDOMNode(this.refs.checkbox).checked = this.props.node[defCol.Checked];
                this.props.refresh(chck);
                if (settings.CallBack.CheckNodeFn) {
                    settings.CallBack.CheckNodeFn(this.props.node, this);
                }
            },
            selectedNodeFn: function(e) {
                settings.CallBack.selectedNode = this.props.node;

                if (selectedScope) {
                    selectedScope.forceUpdate();
                }

                if (settings.CallBack.SelectedFn) {
                    settings.CallBack.SelectedFn(this.props.node, this);
                }
                selectedScope = this;
                this.forceUpdate();
                if (this.props.forceUp) {
                    this.props.forceUp();
                }
            },
            selectedFn: function(node, scope) {
                settings.CallBack.selectedNode = node;

                if (selectedScope) {
                    selectedScope.forceUpdate();
                }
                selectedScope = scope;

                this.forceUpdate();
                if (this.props.forceUp) {
                    this.props.forceUp();
                }
            },
            dblClickNodeFn: function() {
                settings.CallBack.selectedNode = this.props.node;

                if (selectedScope) {
                    selectedScope.forceUpdate();
                }

                if (settings.CallBack.DblClickNode) {
                    settings.CallBack.DblClickNode(this.props.node, this);
                }
                selectedScope = this;
                this.forceUpdate();
            },
            editNode: function(scope) {
                EditNode(this);
            },
            delNode: function(scope) {
                DelNode(this);
            },
            refreshNodeFn: function() {
                this.props.forceUp();
            },
            render: function() {
                var node = this.props.node;
                var classSelect, boxcontrol, editRegionSite, deleteRegionSite, checkbox, checkboxclass = "node-content i-checks";
                var classCollapsed = 'fa icon-null';

                if (settings.CallBack.selectedNode && settings.CallBack.selectedNode[defCol.Id] === this.props.node[defCol.Id] && settings.CallBack.selectedNode[defCol.Type] === this.props.node[defCol.Type]) {
                    classSelect = 'select';
                }

                var classText = settings.Item.IsShowIcon === true ? this.props.Icon : '';

                var textConent = (
                    React.createElement("a", { className: "btn btn-xs", onClick: this.selectedNodeFn.bind(this), onDoubleClick: this.dblClickNodeFn.bind(this) },
                        React.createElement("i", { className: classText }),
                        React.createElement(ContentHightLight, { node: this.props.node, filterText: this.props.filterText })
                    )
                );

                editRegionSite = settings.Item.IsShowEditButton === true ? (React.createElement("a", { className: "btn btn-xs edit", onClick: this.editNode.bind(this) }, React.createElement("i", { className: settings.Icon.ItemEdit }))) : editRegionSite;

                deleteRegionSite = settings.Item.IsShowDelButton === true ? (React.createElement("a", { className: "btn btn-xs del", onClick: this.delNode.bind(this) }, React.createElement("i", { className: settings.Icon.ItemDel }))) : deleteRegionSite;

                var labelcheckbox;
                if (settings.Node.IsShowCheckBox === true) {
                    checkboxclass += ' cmscheckbox';
                    var checkid = 'item-' + this._reactInternalInstance._rootNodeID + node[defCol.Type] + '.' + node[defCol.Id];
                    var checkstats = this.props.node[defCol.Checked] === true ? true : false;
                    checkbox = (React.createElement("input", { type: "checkbox", id: checkid, checked: checkstats, ref: "checkbox", onClick: this.checkFn.bind(this, node, this.props.parentNode) }));
                    labelcheckbox = (React.createElement("label", { htmlFor: checkid }, React.createElement("i", { className: " icon-ok-1" })));
                }

                boxcontrol = settings.Item.IsShowItemMenu === true ? (
                    React.createElement("b", { className: "control-group pull-right" },
                        editRegionSite,
                        deleteRegionSite
                    )
                ) : boxcontrol;


                return (React.createElement("div", { className: classSelect },
                    React.createElement("div", { className: "tree-node-content" },
                        React.createElement("a", null, React.createElement("i", { className: classCollapsed })),
                        React.createElement("span", { className: checkboxclass },
                            checkbox,
                            labelcheckbox,
                            textConent,
                            boxcontrol
                        )
                    )
                ));
            }
        });


        function isSelectInside(node, se) {

            if (!node) {
                return false;
            }

            if (se) {
                var ischild = finditem(node, se[defCol.Type], se[defCol.Id]);
                if (ischild) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        function finditem(findnode, type, id) {

            if (!findnode || type === undefined || id === undefined) return false;

            if (findnode[defCol.Type] && findnode[defCol.Type] === type && findnode[defCol.Id] && findnode[defCol.Id] === id) {
                return findnode;
            }

            var len = findnode[defCol.Childs].length;
            var result = null;
            for (var i = 0; i < len; i++) {
                var s = findnode[defCol.Childs][i];
                if (s[defCol.Childs].length > 0) {
                    var rt = finditem(s, type, id);
                    if (rt) {
                        result = rt;
                        break;
                    }
                } else {
                    if (s[defCol.Type] === type && s[defCol.Id] === id) {
                        result = s;
                        break;
                    }
                }
            }
            return result;
        }

        var Tree = React.createClass({
            displayName: "Tree",
            getInitialState: function() {
                var col;
                if (this.props.showRootNode) {
                    col = false;
                } else {
                    col = settings.Node.InitNode;
                }
                return {
                    //node: node,
                    collapsed: col
                }
            },
            forceUp: function() {
                this.forceUpdate();
                if (this.props.forceUp) {
                    this.props.forceUp();
                }
            },
            refreshFn: function(node) {

                node = this.props.node;
                if (settings.Node.IsShowCheckBox === true) {
                    var nodes = node[defCol.Childs];
                    setStatusCheck(node, nodes);
                }

                this.forceUpdate();

                if (this.props.refresh) {
                    this.props.refresh(node);
                }
            },
            toggleCollapsed: function(collapsed) {

                //setOpNode(this.props.node[defCol.Id], collapsed)
                this.setState({
                    collapsed: collapsed
                });
            },
            render: function() {

                var showRootNode = this.props.showRootNode !== undefined ? this.props.showRootNode : true;
                var isRootNode = false;
                if (this.props.showRootNode === undefined) {
                    this.state.collapsed = settings.Node.IsCollapsedAll === null ? this.state.collapsed : settings.Node.IsCollapsedAll;
                } else {
                    isRootNode = true;
                    if (showRootNode === false) this.state.collapsed = false;
                }
                var collapsed = this.state.collapsed;
                var snode = settings.CallBack.selectedNode;
                var node = this.props.node;
                if (autoselect === true && snode && snode[defCol.Id] !== node[defCol.Id]) {

                    var rs = isSelectInside(node, snode);
                    if (rs) {
                        this.state.collapsed = false;
                        collapsed = this.state.collapsed;
                    }
                }

                var content, nodechild, Icon;

                var scope = this;
                //var node = this.props.node;

                switch (node[defCol.Type]) {
                case NodeType.Folder:
                {
                    if (settings.CallBack.SetIconFolder && typeof settings.CallBack.SetIconFolder === 'function') {
                        var icongroup = settings.CallBack.SetIconFolder(node, this.props.parentNode);
                        Icon = icongroup;
                    } else {
                        Icon = {
                            Expand: settings.Icon.NodeExpand,
                            Collapsed: settings.Icon.NodeCollpased
                        };
                    }


                    if (showRootNode === true) {
                        content = (React.createElement(TreeNodeContent, { node: node, parentNode: this.props.parentNode, forceUp: scope.forceUp.bind(scope), isRootNode: isRootNode, Icon: Icon, toggleCollapsed: scope.toggleCollapsed, collapsed: collapsed, refresh: scope.refreshFn, filterText: scope.props.filterText }));
                    }
                    nodechild = (React.createElement(TreeNode, { node: node, parentNode: this.props.parentNode, forceUp: scope.forceUp.bind(scope), collapsed: collapsed, toggleCollapsed: scope.toggleCollapsed, filterText: scope.props.filterText, refresh: scope.refreshFn }));
                    break;

                }
                case NodeType.File:
                {

                    if (settings.CallBack.SetIconFile && typeof settings.CallBack.SetIconFile === 'function') {
                        var icongroup = settings.CallBack.SetIconFile(node, this.props.parentNode);
                        Icon = icongroup;
                    } else {
                        Icon = settings.Icon.Item;
                    }
                    content = (React.createElement(FileNodeContent, { node: node, Icon: Icon, parentNode: this.props.parentNode, forceUp: scope.forceUp.bind(scope), toggleCollapsed: scope.toggleCollapsed, collapsed: collapsed, refresh: scope.refreshFn, filterText: scope.props.filterText }));
                    break;
                }
                default:
                {

                    if (settings.CallBack.SetIconGroup && typeof settings.CallBack.SetIconGroup === 'function') {
                        var icongroup = settings.CallBack.SetIconGroup(node, this.props.parentNode);
                        Icon = icongroup;
                    } else {
                        Icon = {
                            Expand: settings.Icon.GroupExpand,
                            Collapsed: settings.Icon.GroupCollpased
                        };
                    }
                    content = (React.createElement(TreeNodeContent, { node: node, Icon: Icon, parentNode: this.props.parentNode, forceUp: scope.forceUp.bind(scope), toggleCollapsed: scope.toggleCollapsed, collapsed: collapsed, refresh: scope.refreshFn, filterText: scope.props.filterText }));
                    nodechild = (React.createElement(TreeNode, { node: node, parentNode: this.props.parentNode, forceUp: scope.forceUp.bind(scope), collapsed: collapsed, toggleCollapsed: scope.toggleCollapsed, filterText: scope.props.filterText, refresh: scope.refreshFn }));
                    break;
                }
                }

                return (React.createElement("groupNode", null,
                    content,
                    nodechild
                ));
            }
        });

        element = el;

        settings = JSON.parse(JSON.stringify(opNode));
        NodeType = JSON.parse(JSON.stringify(NType));
        defCol = JSON.parse(JSON.stringify(defOriginal));

        contructTree(def, options);

        function contructTree(def, options) {
            if (def) {
                if (!def.Model) {
                    return;
                }
                defCol.Id = def.Id && def.Id !== "" ? def.Id : defCol.Id;
                defCol.Name = def.Name && def.Name !== "" ? def.Name : defCol.Name;
                defCol.Type = def.Type && def.Type !== "" ? def.Type : defCol.Type;
                defCol.Childs = def.Childs && def.Childs !== "" ? def.Childs : defCol.Childs;
                defCol.Checked = def.Checked && def.Checked !== "" ? def.Checked : defCol.Checked;
                defCol.Count = def.Count && def.Count !== "" ? def.Count : defCol.Count;
                defCol.Model = def.Model ? def.Model : defCol.Model;
            }

            if (options) {
                if (options.Node) {
                    var node = options.Node;
                    settings.Node.IsShowIcon = node.IsShowIcon !== undefined ? node.IsShowIcon : settings.Node.IsShowIcon;
                    settings.Node.IsDraggable = node.IsDraggable !== undefined ? node.IsDraggable : settings.Node.IsDraggable;
                    settings.Node.IsDraggableFile = node.IsDraggableFile !== undefined ? node.IsDraggableFile : settings.Node.IsDraggableFile;
                    settings.Node.IsShowCheckBox = node.IsShowCheckBox !== undefined ? node.IsShowCheckBox : settings.Node.IsShowCheckBox;
                    settings.Node.IsShowRootNode = node.IsShowRootNode !== undefined ? node.IsShowRootNode : settings.Node.IsShowRootNode;
                    settings.Node.IsShowNodeMenu = node.IsShowNodeMenu !== undefined ? node.IsShowNodeMenu : settings.Node.IsShowNodeMenu;
                    settings.Node.IsShowAddNodeButton = node.IsShowAddNodeButton !== undefined ? node.IsShowAddNodeButton : settings.Node.IsShowAddNodeButton;
                    settings.Node.IsShowAddItemButton = node.IsShowAddItemButton !== undefined ? node.IsShowAddItemButton : settings.Node.IsShowAddItemButton;
                    settings.Node.IsShowEditButton = node.IsShowEditButton !== undefined ? node.IsShowEditButton : settings.Node.IsShowEditButton;
                    settings.Node.IsShowDelButton = node.IsShowDelButton !== undefined ? node.IsShowDelButton : settings.Node.IsShowDelButton;
                    settings.Node.IsShowCount = node.IsShowCount !== undefined ? node.IsShowCount : settings.Node.IsShowCount;
                }
                if (options.Item) {
                    var node = options.Item;
                    settings.Item.IsShowItemMenu = node.IsShowItemMenu !== undefined ? node.IsShowItemMenu : settings.Item.IsShowItemMenu;
                    settings.Item.IsShowIcon = node.IsShowIcon !== undefined ? node.IsShowIcon : settings.Item.IsShowIcon;
                    settings.Item.IsHightlightFilter = node.IsHightlightFilter !== undefined ? node.IsHightlightFilter : settings.Item.IsHightlightFilter;
                    settings.Item.IsRadio = node.IsRadio !== undefined ? node.IsRadio : settings.Item.IsRadio;
                    settings.Item.IsAllowFilter = node.IsAllowFilter !== undefined ? node.IsAllowFilter : settings.Item.IsAllowFilter;

                }
                if (options.Icon) {

                    var node = options.Icon;
                    settings.Icon.Item = node.Item ? node.Item : settings.Icon.Item;
                    settings.Icon.RootIcon = node.RootIcon ? node.RootIcon : settings.Icon.RootIcon;
                    settings.Icon.Expand = node.Expand ? node.Expand : settings.Icon.Expand;
                    settings.Icon.Collpased = node.Collpased ? node.Collpased : settings.Icon.Collpased;
                    settings.Icon.NodeExpand = node.NodeExpand ? node.NodeExpand : settings.Icon.NodeExpand;
                    settings.Icon.GroupExpand = node.GroupExpand ? node.GroupExpand : settings.Icon.GroupExpand;
                    settings.Icon.NodeCollpased = node.NodeCollpased ? node.NodeCollpased : settings.Icon.NodeCollpased;
                    settings.Icon.GroupCollpased = node.GroupCollpased ? node.GroupCollpased : settings.Icon.GroupCollpased;
                    settings.Icon.NodeAdd = node.NodeAdd ? node.NodeAdd : settings.Icon.NodeAdd;
                    settings.Icon.NodeAddItem = node.NodeAddItem ? node.NodeAddItem : settings.Icon.NodeAddItem;
                    settings.Icon.NodeEdit = node.NodeEdit ? node.NodeEdit : settings.Icon.NodeEdit;
                    settings.Icon.NodeDel = node.NodeDel ? node.NodeDel : settings.Icon.NodeDel;
                    settings.Icon.ItemDel = node.ItemDel ? node.ItemDel : settings.Icon.ItemDel;
                    settings.Icon.ItemEdit = node.ItemEdit ? node.ItemEdit : settings.Icon.ItemEdit;
                }

                if (options.CallBack) {
                    var node = options.CallBack;

                    settings.CallBack.selectedNode = node.selectedNode ? node.selectedNode : settings.CallBack.selectedNode;
                    settings.CallBack.filterText = node.filterText ? node.filterText : '';
                    settings.CallBack.AddNode = node.AddNode && typeof node.AddNode === 'function' ? node.AddNode : settings.CallBack.AddNode;
                    settings.CallBack.EditNode = node.EditNode && typeof node.EditNode === 'function' ? node.EditNode : settings.CallBack.EditNode;
                    settings.CallBack.DelNode = node.DelNode && typeof node.DelNode === 'function' ? node.DelNode : settings.CallBack.DelNode;
                    settings.CallBack.SelectedFn = node.SelectedFn && typeof node.SelectedFn === 'function' ? node.SelectedFn : settings.CallBack.SelectedFn;
                    settings.CallBack.DblClickNode = node.DblClickNode && typeof node.DblClickNode === 'function' ? node.DblClickNode : settings.CallBack.DblClickNode;
                    settings.CallBack.CheckNodeFn = node.CheckNodeFn && typeof node.CheckNodeFn === 'function' ? node.CheckNodeFn : settings.CallBack.CheckNodeFn;
                    settings.CallBack.DragStart = node.DragStart && typeof node.DragStart === 'function' ? node.DragStart : settings.CallBack.DragStart;
                    settings.CallBack.DragOver = node.DragOver && typeof node.DragOver === 'function' ? node.DragOver : settings.CallBack.DragOver;
                    settings.CallBack.DragEnd = node.DragEnd && typeof node.DragEnd === 'function' ? node.DragEnd : settings.CallBack.DragEnd;
                    settings.CallBack.SetIconFolder = node.SetIconFolder && typeof node.SetIconFolder === 'function' ? node.SetIconFolder : settings.CallBack.SetIconFolder;
                    settings.CallBack.SetIconGroup = node.SetIconGroup && typeof node.SetIconGroup === 'function' ? node.SetIconGroup : settings.CallBack.SetIconGroup;
                    settings.CallBack.SetIconFile = node.SetIconFile && typeof node.SetIconFile === 'function' ? node.SetIconFile : settings.CallBack.SetIconFile;
                }

                if (options.Type) {

                    NodeType.Folder = options.Type.Folder ? options.Type.Folder : NodeType.Folder;
                    NodeType.Group = options.Type.Group ? options.Type.Group : NodeType.Group;
                    NodeType.File = options.Type.File ? options.Type.File : NodeType.File;
                }
            }

            RenderTree(element);
        }


        function RenderTree(element) {
            //settings.CallBack.selectedNode = {};
            if (settings.Node.IsShowCheckBox) {
                checkinglist(defCol.Model);
            }
            React.render(React.createElement(Tree, { node: defCol.Model, filterText: settings.CallBack.filterText, showRootNode: settings.Node.IsShowRootNode }), element)
        }

        this.refesh = function(model, sNode) {
            defCol.Model = model;
            if (sNode) {
                autoselect = true;
                settings.CallBack.selectedNode = sNode;
            }

            RenderTree(element);
            autoselect = false;
        }

        this.filter = function(filterText, model) {
            settings.CallBack.filterText = filterText ? filterText : '';
            settings.Node.IsCollapsedAll = false;
            defCol.Model = model;
            RenderTree(element);
            settings.Node.IsCollapsedAll = null;
        }

        this.expandAll = function() {
            settings.Node.IsCollapsedAll = false;
            RenderTree(element);
            settings.Node.IsCollapsedAll = null;
        }

        this.collapsedAll = function() {
            settings.Node.IsCollapsedAll = true;
            RenderTree(element);
            settings.Node.IsCollapsedAll = null;
        }

        this.destroy = function() {

            element = null;

            // Remove draggable attributes
            Array.prototype.forEach.call(el.querySelectorAll('[draggable]'), function(el) {
                el.removeAttribute('draggable');
            });

        }

        function checkinglist(node) {

            if (!node) return null;

            var result = false;
            var intermid = false;
            var interval = false;
            var i = 0;
            if (node[defCol.Childs] && node[defCol.Childs].length > 0) {

                node[defCol.Childs].forEach(function(n) {
                    var ch = n[defCol.Checked];
                    if (n[defCol.Childs] && n[defCol.Childs].length > 0) {
                        ch = checkinglist(n);
                    }

                    if (i === 0) {
                        interval = ch;
                    }

                    if (i > 0 && interval !== ch) {
                        intermid = true;
                    }

                    if (intermid === true && i > 0) {
                        ch = null;
                    }

                    result = ch;
                    i++;
                });

            }
            else {
                result = node[defCol.Checked];
            }
            node[defCol.Checked] = result;
            return result;
        }


        function AddNode(scope) {

            if (!settings.CallBack.AddNode) {
                return;
            }

            settings.CallBack.AddNode(scope.props.node, scope);
        }

        function EditNode(scope) {
            if (!settings.CallBack.EditNode) {
                return;
            }
            settings.CallBack.EditNode(scope.props.node, scope);
        }

        function DelNode(scope) {
            if (!settings.CallBack.DelNode) {
                return;
            }
            settings.CallBack.DelNode(scope.props.node, scope);
        }

        function SelectedFn(scope) {
            if (!settings.CallBack.SelectedFn) {
                return;
            }
            settings.CallBack.SelectedFn(scope.props.node, scope);
        }

        function CbDragover(e, scope, dragged, eventElm) {

            var contDrop = true;
            if (settings.CallBack.DragOver && typeof settings.CallBack.DragOver === 'function') {
                contDrop = settings.CallBack.DragOver(e, scope, dragged, eventElm);
            }
            return contDrop;
        }

        function CbDragend(e, dragObj, dropObj, from, to, isSameScope) {
            if (settings.CallBack.DragEnd && typeof settings.CallBack.DragEnd === 'function') {
                settings.CallBack.DragEnd(e, dragObj, dropObj, from, to, isSameScope);
            }
        }

        function CbDragstart(e, scope) {
            if (settings.CallBack.DragStart && typeof settings.CallBack.DragStart === 'function') {
                settings.CallBack.DragStart(e, scope);
            }
        }

        function holderPlace() {
            if (!placeholder) {
                var pl = document.createElement('li');
                pl.className = 'placeholder';
                pl.setAttribute("data-drag-handle", "true");
                //var att = document.createAttribute("data-drag-handle");
                //att.value = "true";
                //pl.setAttributeNode(att);
                placeholder = pl;
            }
            return placeholder;
        }

        function dropReraw(from, to) {
            dropData.props.node[defCol.Childs].splice(to, 0, dragData.props.node[defCol.Childs].splice(from, 1)[0])
            dropData.props.forceUp();

            //console.log(dropData.props.node);
        }

        function checkNodeStatus(chck, refChk) {

            if (settings.Item.IsRadio === true) {
                return;
            }

            switch (chck[defCol.Checked]) {
            case true:
            {
                checkChildNode(false, chck[defCol.Childs]);
                chck[defCol.Checked] = false;
                React.findDOMNode(refChk).checked = false;
                break;
            }
            default:
            {
                checkChildNode(true, chck[defCol.Childs]);
                chck[defCol.Checked] = true;
                React.findDOMNode(refChk).checked = true;
                break;
            }
            }
        }

        function checkChildNode(ck, nodes) {
            var leNodes = nodes.length
            for (var i = 0; i < leNodes; i++) {
                if (nodes[i].isShow === undefined || nodes[i].isShow && nodes[i].isShow === true) nodes[i][defCol.Checked] = ck;
                if (nodes[i][defCol.Childs].length > 0) {
                    checkChildNode(ck, nodes[i][defCol.Childs]);
                }
            }
        }

        function findFirstCheck(nodes) {
            var result = nodes[0];
            for (var i = 0; i < nodes.length; i++) {
                if ((nodes[i].isShow === undefined || nodes[i].isShow && nodes[i].isShow === true)) {
                    result = nodes[i];
                    break;
                }
            }
            return result;
        }

        function setStatusCheck(node, nodes) {
            if (node[defCol.Type] !== NodeType.File) {
                var leNodes = nodes.length;

                if (leNodes > 0) {
                    var state = findFirstCheck(nodes)[defCol.Checked];
                    for (var i = 1; i < leNodes; i++) {
                        if (nodes[i][defCol.Checked] !== state && (nodes[i].isShow === undefined || nodes[i].isShow && nodes[i].isShow === true)) {
                            state = null;
                            break;
                        }
                    }
                    node[defCol.Checked] = state;
                } else {
                    node[defCol.Checked] = node[defCol.Checked];
                }
            }
        }

        function dragEndFn(scope, e) {
            e.preventDefault();
            if (e.stopPropagation) {
                e.stopPropagation();
            } else {
                e.cancelBubble = true;
            }

            dragged.style.display = "block";

            if (allowDrop === false) {
                holderPlace().style.display = "none";
                return;
            }

            over.parentElement.removeChild(holderPlace());

            //var from = Number(dragged.dataset.id);
            var from = Number(dragged.getAttribute('data-id'));
            //console.log(from);                    
            //var to = Number(over.dataset.id);
            var to = Number(over.getAttribute('data-id'));
            //console.log(to); 

            if (dragData.props.node[defCol.Id] === dropData.props.node[defCol.Id]) {
                if (from < to) to--;
                if (e.InBefore === false) to++;
                CbDragend(e, dragData.props.node, dropData.props.node, from, to, true);
                scope.props.node[defCol.Childs].splice(to, 0, scope.props.node[defCol.Childs].splice(from, 1)[0]);


            } else {
                CbDragend(e, dragData.props.node, dropData.props.node, from, to, false);
                dropReraw(from, to, e);
            }

            scope.props.forceUp();
        }

        function dragOverFn(scope, e) {
            e.preventDefault();
            e.stopPropagation();


            dropData = scope;
            var targetRow = e.target;

            dragged.style.display = "none";
            if (targetRow.className == "placeholder") {
                return;
            }

            var chekceventElm = e.target
            var eventElm = e.target
            while (eventElm && (eventElm.getAttribute("data-drag-handle") !== "true")) {
                eventElm = eventElm.parentElement;
            }

            if (!eventElm) {

                return;
            }

            over = eventElm;

            if (eventElm.getAttribute("data-drag-handle") !== "true") {
                return;
            }

            allowDrop = false;

            var from = Number(dragged.getAttribute('data-id'));
            var contDrop = CbDragover(e, scope, dragDataNode[defCol.Childs][from], eventElm);

            if (contDrop === false) {


                return;
            }

            var posEventEl = e.pageY - findOffsetTop(eventElm); // 120 - eventElm.offsetTop;
            //var posEventEl = e.pageY - eventElm.offsetTop;

            var heightOfOver = eventElm.offsetHeight / 2;
            var parent = eventElm.parentElement;

            //console.log('Y: ' + posEventEl + ' offsetTop' + '???' + heightOfOver);
            if (posEventEl > heightOfOver) {
                parent.insertBefore(holderPlace(), eventElm.nextElementSibling);
                e.InBefore = false;
            } else if (posEventEl < heightOfOver) {
                parent.insertBefore(holderPlace(), eventElm);
                e.InBefore = true;
            }

            allowDrop = true;
        }

        function findOffsetTop(elm) {
            var result = 0;
            if (elm.offsetParent) {
                do {
                    result += elm.offsetTop;
                } while (elm = elm.offsetParent);
            }
            return result;
        }

        function dragStartFn(scope, e, node) {

            e.stopPropagation();
            holderPlace().style.display = 'block';
            allowDrop = false;

            dragged = e.currentTarget;
            //e.dataTransfer.effectAllowed = 'move';
            dragData = scope;
            dragDataNode = JSON.parse(JSON.stringify(scope.props.node));

            //            e.dataTransfer.setData("text", JSON.stringify(node));
            var tranferJson = JSON.stringify(node);
            var format = "text/html";
            try {
                e.dataTransfer.setData(format, tranferJson);
            } catch (ex) {
                e.dataTransfer.setData("text", tranferJson);
            }

            e.dataTransfer.effectAllowed = "copy";

            CbDragstart(e, scope);
        }


    }

    CMSTree.create = function(el, def, options) {
        return new CMSTree(el, def, options);
    };

    return CMSTree;
});