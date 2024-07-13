var dataTable;

$(document).ready(function () {
    loadDataTable()
});

function loadDataTable() {
    dataTable = $("#tblData").DataTable({
        "ajax": {
            "url": "/Order/GetList"
        },
        "columns": [
            { "data": "id", "width": "2%" },
            { "data": "fullName", "width": "10%" },
            { "data": "phoneNumber", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Order/Details/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                <i class="fa fa-edit"></i>
                            </a>
                        </div>
                    `;
                },
                "width": "4%"
            }
        ]
    });
}
