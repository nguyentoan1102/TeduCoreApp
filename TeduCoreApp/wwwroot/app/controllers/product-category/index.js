class productCategoryController {
    constructor() {
        this.initialize = function () {
            loadData();
        };

        function loadData() {
            $.ajax({
                type: "get",
                url: "/admin/ProductCategory/GetAll",
                data: "",
                dataType: "json",
                success: function (response) {
                    var data = [];
                    $.each(response, function (i, item) {
                        data.push({
                            id: item.Id,
                            text: item.Name,
                            parentId: item.ParentId,
                            sortOrder: item.SortOrder

                        });
                    });
                    var treeArr = tedu.unflattern(data);
                    $('#treeProductCategory').tree({
                        data: treeArr,
                        dnd: true
                    });

                }
            });
        };
    }
}