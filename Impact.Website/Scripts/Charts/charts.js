function drawColumnChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.ColumnChart(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
    $('#toggle-normalized').change(function () {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function drawMaterialBarOrColumnChart (chartModel) {
    var data = getDataToDraw(chartModel);
    var options = google.charts.Bar.convertOptions(chartModel.options);

    var chart = new google.charts.Bar(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
    $('#toggle-normalized').change(function () {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function drawPieChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.PieChart(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
    $('#toggle-normalized').change(function () {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function drawGaugeChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.Gauge(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
    $('#toggle-normalized').change(function () {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function getDataToDraw(chartModel) {
    var showNormalized = $('#toggle-normalized').prop('checked');
    var showAllWeeks = $('#toggle-allWeeks').prop('checked');

    var rawWeeksDefined = chartModel.rawWeeks !== undefined;
    var normalizedPreviousWeeksDefined = chartModel.normalizedPreviousWeeks !== undefined;
    var normalizedAllWeeksDefined = chartModel.normalizedAllWeeks !== undefined;
    
    var json;
    if (!rawWeeksDefined) {
        if (showNormalized && showAllWeeks)
            json = chartModel.normalizedAllWeeks;
        else
            json = chartModel.normalizedPreviousWeeks;
    } else if (!normalizedAllWeeksDefined && !normalizedPreviousWeeksDefined) {
        json = chartModel.rawWeeks;
    } else {
        if (showAllWeeks && showNormalized)
            json = chartModel.normalizedAllWeeks;
        else if (showNormalized) {
            if (normalizedPreviousWeeksDefined)
                json = chartModel.normalizedPreviousWeeks;
            else
                json = chartModel.normalizedAllWeeks;
        } else
            json = chartModel.rawWeeks;
    }

    return new google.visualization.arrayToDataTable(json);
}