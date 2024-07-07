$(document).ready(function () {
    $('#cityInput').keyup(function () {
        var query = $(this).val();
        $.ajax({
            url: '/Cart/SearchCities',
            method: 'GET',
            data: { query: query },
            success: function (data) {
                $('#cityList').empty();
                $.each(data, function (index, value) {
                    $('#cityList').append('<li class="city list-group-item">' + value + '</li>');
                });
            }
        });
    });
    $('#warehouseInput').prop('disabled', true);
    $('#warehouseInput').keyup(function () {
        var query = $(this).val();
        var city = $('#cityInput').val();
        if (query) {
            $.ajax({
                url: '/Cart/GetWarehouses',
                method: 'GET',
                data: { city: city, query: query },
                success: function (data) {
                    $('#warehouseList').empty();
                    $.each(data, function (index, value) {
                        $('#warehouseList').append('<li class="warehouse list-group-item">' + value + '</li>');
                    });
                }
            });
        }
    });


    $(document).on('click', '.city', function () {
        var cityName = $(this).text();
        $('#cityInput').val(cityName);
        $('#cityList').empty();

        $('#warehouseInput').prop('disabled', false);

        $.ajax({
            url: '/Cart/GetWarehouses',
            method: 'GET',
            data: { city: cityName },
            success: function (data) {
                $('#warehouseList').empty();
                $.each(data, function (index, value) {
                    $('#warehouseList').append('<li class="warehouse list-group-item">' + value + '</li>');
                });
            }
        });
    });

    $(document).on('click', '.warehouse', function () {
        var warehouseName = $(this).text();
        $('#warehouseInput').val(warehouseName);
        $('#warehouseList').empty();
    });
});