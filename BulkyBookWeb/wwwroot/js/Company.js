﻿var datatable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    datatable = $('#tblData').DataTable({
        "ajax": { url:'/admin/company/getall'},
        "columns": [            
            { data: 'id', "width": "5%" },
            { data: 'name', "width": "20%" },           
            { data: 'streetAddress', "width": "20%"},
            { data: 'city', "width": "10%"},
            { data: 'state', "width": "10%"},
            { data: 'postalAddress', "width": "10%" },
            { data: 'phoneNumber', "width": "10%" },           
            {             
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                            <a href="/admin/company/upsert?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square" ></i>Edit</a>
                        <a onClick=Delete('/admin/company/delete/${data}') class="btn btn-danger mx-2">
                            <i class="bi bi-trash"></i>Delete</a>
                        
                            </div>`
                },
                "width": "10%"
            }
                
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    datatable.ajax.reload();
                    toastr.success(data.massage, "Delete Success");
                }
            })
           
        }
    });
}
//----NOTICE------------//
//*//not space gap between in href="/admin/product/upsert?id=${data}"
//or
//*//onClick=Delete('/admin/product/delete/${data}')