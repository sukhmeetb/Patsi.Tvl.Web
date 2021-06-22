function showModal() {
    if (flagstr == 1) {
        $('#savePopup').modal('show');
        
    }
    else if (flagstr == -1) {
        console.log("reach")
        document.getElementById("Creationpopup").innerHTML = "New User was not created";
        $('#savePopup').modal('show');
    }
}

