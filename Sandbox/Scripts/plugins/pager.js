



(function ($) {
    $.fn.pager = function (options)
    {
        var settings = $.extend({
            // These are the defaults.

            numTabs: this.length            
        }, options);

        

        // listeners       
        //$this.on('....', function () {
                
        //});

        return this;
    };

}(jQuery));

function UpdateRecipientPageNumbers(page, numpages, pageNumberList) {
    numpages = parseInt(numpages, 0);
    page = parseInt(page, 0);
    var $pageNumberList = $(pageNumberList);
    var $pageNumbers = $pageNumberList.find('.page-number');

    $pageNumbers.eq(4).attr('data-value', numpages);
    $pageNumbers.removeClass("active");
    $pageNumberList.find('li').removeClass('hidden');

    // if on first three pages
    if (page <= 3) {
        for (var i = 0; i < Math.min(numpages, 5) ; i++)
            $pageNumbers.eq(i).text(i + 1);
        for (var j = numpages; j < 5; j++)
            $pageNumbers.eq(j).text(' ');
        if (numpages > 5)
            $pageNumbers.eq(4).text('...');
    }
    else if (numpages - page < 3) // if on last three pages
    {
        for (var i = 0; i < Math.min(numpages, 5) ; i++)
            $pageNumbers.eq(4 - i).text(numpages - i);
        for (var j = numpages; j < 5; j++)
            $pageNumbers.eq(4 - j).text(' ');
        if (numpages > 5)
            $pageNumbers.eq(0).text('...');
    }
    else    // somewhere in the middle
    {
        $pageNumbers.eq(0).text('...');
        for (var i = 1; i < Math.min(numpages - page + 3, 4) ; i++)
            $pageNumbers.eq(i).text(i + page - 2);
        $pageNumbers.eq(4).text('...');
    }

    $pageNumbers.each(function () {
        var $this = $(this);
        if ($this.text() != '...' && $this.text() != ' ') {
            $this.attr('data-value', $this.text());
        }
    });

    $pageNumberList.find('.page-left').prop('disabled', page == 1);

    $pageNumberList.find('li:contains(" ")').not('.nopad').addClass('hidden');
    $pageNumberList.find('li:contains(' + page + ')').addClass('active');
}





