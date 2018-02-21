function drawGraphs(balance, weeks, pie, potential) {
    
    google.charts.load('current', { packages: ['corechart', 'bar', 'gauge'] });
    google.charts.setOnLoadCallback(function() { 
        drawBalanceChart(balance); 
        drawWeeksChart(weeks); 
        drawPieChart(pie); 
        drawGaugeChart(potential);
    });
}

function drawBalanceChart (balance) {
    var data = google.visualization.arrayToDataTable(balance.json);

    var options = {
        chart: {
            title: balance.title,
            subtitle: balance.subTitle
        },
        height: 170,
        colors: [balance.color],
        hAxis: {
            viewWindowMode: 'explicit',
            viewWindow: {
                max: balance.xMax,
                min: balance.xMin
            }
        },
        bars: 'horizontal' // Required for Material Bar Charts.
    };

    var chart = new google.charts.Bar(document.getElementById(balance.divId));
    chart.draw(data, google.charts.Bar.convertOptions(options));
}

function drawWeeksChart(weeks) {
    var options = {
        width: 1600,
        height: 800,
        isStacked: true,
        title: weeks.graphTitle,
        colors: ['#EFEFEF', '#289eff', '#3366cc', '#FF4635', 'orange', 'green'],
        animation:{
            duration: 1000,
            easing: 'out'
        },
        vAxis: {
            title: 'Timer',
            ticks: [2.5, 5, 7.5, 10, 12.5, 15, 17.5, 20, 22.5, 25, 27.5, 30, 32.5, 35, 37.5, 40, 42.5, 45, 47.5, 50, 52.5, 55, 57.5, 60, 62.5, 65, 67.5, 70, 72.5, 75, 77.5, 80, 82.5, 85, 87.5, 90, 92.5, 95, 97.5, 100],
            viewWindowMode: 'explicit',
            viewWindow: {
                max: weeks.yMax,
                min: 0
            }
        },
        hAxis: {
            title: 'Uge'
        }
    };

    var newJson = weeks.isNormalized ? weeks.normalizedJson : weeks.json;
    var data = new google.visualization.arrayToDataTable(newJson);

    var chart = new google.visualization.ColumnChart(document.getElementById(weeks.divId));
    chart.draw(data, options);

    $('#toggle-normalized').change(function() {
        var checked = $(this).prop('checked');

        var newJson = checked ? weeks.normalizedJson : weeks.json;
        var data = new google.visualization.arrayToDataTable(newJson);
        chart.draw(data, options);
    });
}

function drawPieChart(pie) {
    var data = google.visualization.arrayToDataTable(pie.json);

    var options = {
        title: pie.title,
        colors: ['#FF4635', 'orange']
    };

    var chart = new google.visualization.PieChart(document.getElementById(pie.divId));
    chart.draw(data, options);
}

function drawGaugeChart(gauge) {
    var data = google.visualization.arrayToDataTable(gauge.json);
    var options = gauge.options;

    var chart = new google.visualization.Gauge(document.getElementById(gauge.divId));
    chart.draw(data, options);
}