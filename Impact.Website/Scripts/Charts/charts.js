function drawColumnChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.ColumnChart(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function drawMaterialBarOrColumnChart (chartModel) {
    var data = getDataToDraw(chartModel);
    var options = google.charts.Bar.convertOptions(chartModel.options);

    var chart = new google.charts.Bar(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function drawPieChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.PieChart(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function drawGaugeChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.Gauge(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function onToggleChange(chart, chartModel, options) {
    $('#toggle-normalized').change(function () {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function getDataToDraw(chartModel) {
    var json = getRawData(chartModel); 
    return new google.visualization.arrayToDataTable(json);
}

function getRawData(chartModel) {
    var showNormalized = $('#toggle-normalized').prop('checked');
    var showAllWeeks = $('#toggle-allWeeks').prop('checked');

    var rawWeeksDefined = chartModel.rawWeeks !== undefined;
    var normalizedPreviousWeeksDefined = chartModel.normalizedPreviousWeeks !== undefined;
    var normalizedAllWeeksDefined = chartModel.normalizedAllWeeks !== undefined;
    
    //Only raw defined
    if (!normalizedPreviousWeeksDefined && !normalizedAllWeeksDefined) {
        return chartModel.rawWeeks;
    }

    //Only previous defined
    if (!rawWeeksDefined && !normalizedAllWeeksDefined) {
        return chartModel.normalizedPreviousWeeks;
    }
    
    //Only normalized all defined
    if (!rawWeeksDefined && !normalizedPreviousWeeksDefined) {
        return chartModel.normalizedAllWeeks;
    }
    
    //All data sets defined
    if (rawWeeksDefined && normalizedPreviousWeeksDefined && normalizedAllWeeksDefined) {
        if (showNormalized && showAllWeeks) {
            return chartModel.normalizedAllWeeks;
        }
        if (showNormalized) {
            return chartModel.normalizedPreviousWeeks;
        }
        return chartModel.rawWeeks;
    }
    
    // normalized previous and all are defined
    if (!rawWeeksDefined) {
        if (showNormalized && showAllWeeks) {
            return chartModel.normalizedAllWeeks;
        }
        return chartModel.normalizedPreviousWeeks;
    }
    
    // raw and normalized all are defined
    if (!normalizedPreviousWeeksDefined) {
        if (showNormalized) {
            return chartModel.normalizedAllWeeks;
        }
        return chartModel.rawWeeks;
    }
    
    console.log('No dataset could be found. Does the model only have raw and normalized previous?')
}