function updateCitiesOptions(selectedCityId, otherCityId) {
    var selectedCity = document.getElementById(selectedCityId).value;
    var otherCityDropdown = document.getElementById(otherCityId);

    // Enable all options in the other dropdown
    for (var i = 0; i < otherCityDropdown.options.length; i++) {
        otherCityDropdown.options[i].disabled = false;
    }

    // Disable the selected city option in the other dropdown
    for (var i = 0; i < otherCityDropdown.options.length; i++) {
        if (otherCityDropdown.options[i].value === selectedCity) {
            otherCityDropdown.options[i].disabled = true;
            break;
        }
    }
}
