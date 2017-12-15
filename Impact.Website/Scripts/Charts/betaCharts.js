google.charts.load('current', { packages: ['corechart', 'bar'] });
google.charts.setOnLoadCallback(drawWeeksChart);

function drawWeeksChart() {
    var data = new google.visualization.arrayToDataTable([
        ['Uge', 'Work', 'Interessetid', '39-44', '44+'],
        ['Uge 1', 37.5, 1.5, 5, 3],
        ['Uge 2', 37.5, 1.5, 0, 0],
        ['Uge 3', 36, 0, 0, 0],
        ['Uge 4', 37.5, 1.5, 5, 3],
        [null, null, null, null, null],
        ['Uge 5', 37.5, 1.5, 5, 3],
        ['Uge 6', 37.5, 1.5, 0, 0],
        ['Uge 7', 36, 0, 0, 0],
        ['Uge 8', 37.5, 1.5, 5, 3],
        [null, null, null, null, null],
        ['Uge 9', 37.5, 1.5, 5, 3],
        ['Uge 10', 37.5, 1.5, 0, 0],
        ['Uge 11', 36, 0, 0, 0],
        ['Uge 12', 37.5, 1.5, 5, 3]
    ]);

    var options = {
        width: 1000,
        height: 700,
        isStacked: true,
        title: '2 Kvartal. Marts - Maj',
        vAxis: {
            title: 'Timer',
            ticks: [20, 22.5, 25, 27.5, 30, 32.5, 35, 37.5, 40, 42.5, 45, 47.5, 50],
            viewWindowMode: 'explicit',
            viewWindow: {
                max: 50,
                min: 20
            }
        },
        hAxis: {
            title: 'Uge'
        }
    };

    var chart = new google.visualization.ColumnChart(document.getElementById('chart_div1'));
    chart.draw(data, options);
}

function drawGroupStacked() {
    var data = new google.visualization.arrayToDataTable([
        ['Måned', 'Uge 1 - Work', 'Uge 1 - Interessetid', 'Uge 1 - 39-44', 'Uge 1 - 44+',
            'Uge 2 - Work', 'Uge 2 - Interessetid', 'Uge 2 - 39-44', 'Uge 2 - 44+',
            'Uge 3 - Work', 'Uge 3 - Interessetid', 'Uge 3 - 39-44', 'Uge 3- 44+',
            'Uge 4 - Work', 'Uge 4 - Interessetid', 'Uge 4 - 39-44', 'Uge 4- 44+'],
        ['Januar', 37.5, 1.5, 5, 3, 37.5, 1.5, 0, 0, 36, 0, 0, 0, 37.5, 1.5, 5, 3],
        ['Februar', 37.5, 1.5, 5, 3, 37.5, 1.5, 0, 0, 36, 0, 0, 0, 37.5, 1.5, 5, 3],
        ['Marts', 37.5, 1.5, 5, 3, 37.5, 1.5, 0, 0, 36, 0, 0, 0, 37.5, 1.5, 5, 3]
    ]);

    var options = {
        height: 700,
        isStacked: true,
        vAxes: {

        },
        vAxis: {
            viewWindow: {
                max: 50,
                min: 30
            }
        },
        seriesType: 'bars',
        series: {
            0: {
                targetAxisIndex: 0
            },
            1: {
                targetAxisIndex: 0
            },
            2: {
                targetAxisIndex: 0
            },
            3: {
                targetAxisIndex: 0
            },
            4: {
                targetAxisIndex: 1
            },
            5: {
                targetAxisIndex: 1
            },
            6: {
                targetAxisIndex: 1
            },
            7: {
                targetAxisIndex: 1
            },
            8: {
                targetAxisIndex: 2
            },
            9: {
                targetAxisIndex: 2
            },
            10: {
                targetAxisIndex: 2
            },
            11: {
                targetAxisIndex: 2
            },
            12: {
                targetAxisIndex: 3
            },
            13: {
                targetAxisIndex: 3
            },
            14: {
                targetAxisIndex: 3
            },
            15: {
                targetAxisIndex: 3
            }
        }
    };

    var chart = new google.charts.Bar(document.getElementById('chart_div2'));
    chart.draw(data, google.charts.Bar.convertOptions(options));
}

function drawGroupStacked2() {
    var data = new google.visualization.arrayToDataTable([
        ['Måned', 'Work', 'Interessetid', '39-44', '44+'],
        ['Januar - Uge 1', 37.5, 1.5, 5, 3],
        ['Januar - Uge 2', 37.5, 1.5, 0, 0],
        ['Januar - Uge 3', 36, 0, 0, 0],
        ['Januar - Uge 4', 37.5, 1.5, 5, 3],
        [null, null, null, null, null],
        ['Februar - Uge 5', 37.5, 1.5, 5, 3],
        ['Februar - Uge 6', 37.5, 1.5, 0, 0],
        ['Februar - Uge 7', 36, 0, 0, 0],
        ['Februar - Uge 8', 37.5, 1.5, 5, 3],
        [null, null, null, null, null],
        ['Marts - Uge 9', 37.5, 1.5, 5, 3],
        ['Marts - Uge 10', 37.5, 1.5, 0, 0],
        ['Marts - Uge 11', 36, 0, 0, 0],
        ['Marts - Uge 12', 37.5, 1.5, 5, 3]
    ]);

    var options = {
        width: 1000,
        height: 700,
        isStacked: true,
        vAxis: {
            viewWindow: {
                max: 50,
                min: 30
            }
        }
    };

    var chart = new google.charts.Bar(document.getElementById('chart_div3'));
    chart.draw(data, google.charts.Bar.convertOptions(options));
}