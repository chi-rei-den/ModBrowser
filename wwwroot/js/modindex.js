var $mod = $('.mod-masonry').masonry({
    // options
    itemSelector: '.mod-item',
    columnWidth: '.mod-item',
    percentPosition: true,
});

$('.collapse').on('shown.bs.collapse hidden.bs.collapse', function () {
    $mod.masonry('layout');
});

$('.collapse').on('show.bs.collapse hidden.bs.collapse', function () {
    var modItem = $(this).parents('.mod-item');

    if (modItem.hasClass('modindex')) {
        modItem.removeClass('modindex');
    } else {
        modItem.addClass('modindex');
    }

    var modSwitchSvg = modItem.find('.mod-switch').find('.bi');

    if (modSwitchSvg.hasClass('bi-chevron-compact-down')) {
        modSwitchSvg.removeClass('bi-chevron-compact-down');
        modSwitchSvg.addClass('bi-chevron-compact-up');
        modSwitchSvg.html('<path fill-rule="evenodd" d="M7.776 5.553a.5.5 0 0 1 .448 0l6 3a.5.5 0 1 1-.448.894L8 6.56 2.224 9.447a.5.5 0 1 1-.448-.894l6-3z"/>');
    } else {
        modSwitchSvg.removeClass('bi-chevron-compact-up');
        modSwitchSvg.addClass('bi-chevron-compact-down');
        modSwitchSvg.html('<path fill-rule="evenodd" d="M1.553 6.776a.5.5 0 0 1 .67-.223L8 9.44l5.776-2.888a.5.5 0 1 1 .448.894l-6 3a.5.5 0 0 1-.448 0l-6-3a.5.5 0 0 1-.223-.67z" />');
    }
});