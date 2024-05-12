var datatable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    datatable = $('#tblData').DataTable({
        "ajax": { url:'/admin/product/getall'},
        "columns": [            
            { data: 'productId', "width": "5%" },
            { data: 'title', "width": "20%" },
            { data: 'isbn', "width": "20%"},
            { data: 'price', "width": "5%"},
            { data: 'author', "width": "20%"},
            { data: 'categoryModel.categoryName', "width": "10%" },           
            {             
                data: 'productId',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                            <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square" ></i>Edit</a>
                        <a onClick=Delete('/admin/product/delete/${data}') class="btn btn-danger mx-2">
                            <i class="bi bi-trash"></i>Delete</a>
                        
                            </div>`
                },
                "width": "20%"
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
// not space gap between in href="/admin/product/upsert?id=${data}"
//or
//onClick=Delete('/admin/product/delete/${data}')