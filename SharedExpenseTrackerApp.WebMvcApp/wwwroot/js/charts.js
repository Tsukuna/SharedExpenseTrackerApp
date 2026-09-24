/**
 * charts.js — ApexCharts helpers for the Expense Summary page.
 */

/**
 * Initialise the paid/unpaid donut chart.
 * @param {string} elementId  - The container div ID
 * @param {number} paidAmount
 * @param {number} unpaidAmount
 */
function initExpenseChart(elementId, paidAmount, unpaidAmount) {
    const el = document.getElementById(elementId);
    if (!el || typeof ApexCharts === 'undefined') return;

    const total = paidAmount + unpaidAmount;

    const options = {
        chart: {
            type: 'donut',
            height: 300,
            fontFamily: 'Inter, sans-serif',
            animations: { enabled: true, speed: 600 }
        },
        series: [paidAmount, unpaidAmount],
        labels: ['Paid', 'Unpaid'],
        colors: ['#10b981', '#ef4444'],
        legend: {
            position: 'bottom',
            fontFamily: 'Inter, sans-serif',
            fontSize: '13px',
            labels: { colors: '#64748b' }
        },
        dataLabels: {
            enabled: true,
            formatter: (val, opts) => {
                const raw = opts.w.config.series[opts.seriesIndex];
                return raw.toLocaleString() + ' ks';
            },
            style: { fontSize: '12px', fontFamily: 'Inter, sans-serif', fontWeight: '600' }
        },
        plotOptions: {
            pie: {
                donut: {
                    size: '65%',
                    labels: {
                        show: true,
                        total: {
                            show: true,
                            label: 'Total',
                            color: '#64748b',
                            fontSize: '13px',
                            fontFamily: 'Inter, sans-serif',
                            formatter: () => total.toLocaleString() + ' ks'
                        }
                    }
                }
            }
        },
        tooltip: {
            y: { formatter: val => val.toLocaleString() + ' ks' }
        },
        stroke: { show: false }
    };

    const chart = new ApexCharts(el, options);
    chart.render();

    return chart;
}
