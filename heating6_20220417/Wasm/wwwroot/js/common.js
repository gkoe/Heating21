window.ShowToastr = (type, message) => {
    if (type === "success") {
        toastr.success(message, "Operation Successful", { timeOut: 10000 });
    }
    if (type === "error") {
        toastr.error(message, "Operation Failed", { timeOut: 10000 });
    }
    if (type === "show") {
        $('#deleteConfirmationModal').modal('show');
    }
    if (type === "hide") {
        $('#deleteConfirmationModal').modal('hide');
    }
}

//window.ShowHideConfirmationModal = ( message) => {
//    if (message === "show") {
//        $('#deleteConfirmationModal').modal('show');
//    }
//    if (message === "hide") {
//        $('#deleteConfirmationModal').modal('hide');    }
//}

//function ShowDeleteConfirmationModal() {
//    $('#deleteConfirmationModal').modal('show');
//}

//function HideDeleteConfirmationModal() {
//    $('#deleteConfirmationModal').modal('hide');
//}