﻿var datatable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess"))
    {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("completed"))
        {
            loadDataTable("completed");
        }
            
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else {
                if (url.includes("approved"))
                {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }               

        }
    }
});

function loadDataTable(status) {
    datatable = $('#tblData').DataTable({
        "ajax": { url:'/admin/order/getall?status='+status},
        "columns": [            
            { data: 'orderHeaderId', "width": "5%" },
            { data: 'name', "width": "20%" },
            { data: 'phoneNumber', "width": "20%"},
            { data: 'applicationUser.email', "width": "5%"},
            { data: 'orderStatus', "width": "20%"},
            { data: 'orderTotal', "width": "20%" },       
            {             
                data: 'orderHeaderId',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                            <a href="/admin/order/details?orderHeaderId=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square" ></i></a>
                                   
                            </div>`
                },
                "width": "20%"
            }
                
        ]
    });
}



// not space gap between in href="/admin/product/upsert?id=${data}"
//or
//onClick=Delete('/admin/product/delete/${data}')