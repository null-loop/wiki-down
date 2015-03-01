(function (wikidown, $, undefined) {
    wikidown.css($("body"));

    var readerModeButton = $("#activateReaderModeButton");
    var editorModeButton = $("#activateEditorModeButton");
    var metaModeButton = $("#activateMetaModeButton");

    var articleReaderContainer = $("#articleReaderContainer");
    var articleMetaContainer = $("#articleMetaContainer");
    var articleEditorContainer = $("#articleEditorContainer");

    var current = articleReaderContainer;

    var metadata = undefined;
    var editor = undefined;

    var articleGlobaId = articleReaderContainer.data("articleglobalid");
    var articlePath = articleReaderContainer.data("articlepath");

    // bring in editor bits
    
    function hideCurrent() {
        current.effect("fade", { direction: "up", mode: "hide" }, 500);
    }

    function showCurrent() {
        current.effect("fade", { direction: "up", mode: "show" }, 500);
    }

    function beginMetaDataLoad() {
        //todo:get html, data, build ko vm, bind, add to dom.
        wikidown.overlay("Loading MetaData", articleMetaContainer);
        if (metadata == undefined) {
            metadata = {};
            $.get(wikidown.uri('html/wikidown.metadata.html'), function (content) {
                articleMetaContainer.html(content);
                $.get(window.wikidown.uri('api/article-meta/g/' + articleGlobaId), function (content) {

                });
                // now get actual meta data and bind it
                wikidown.hideOverlay(articleMetaContainer);
            });
        }

        // todo:set height
    }

    function beginEditorLoad() {
        //todo: bring in existing editor bits - adapat to non 
    }

    function enableReaderMode() {
        readerModeButton.addClass("disabled");
        metaModeButton.removeClass("disabled");
        editorModeButton.removeClass("disabled");
        hideCurrent();
        current = articleReaderContainer;
    }

    function enableMetaMode() {
        metaModeButton.addClass("disabled");
        readerModeButton.removeClass("disabled");
        editorModeButton.removeClass("disabled");
        if (current != articleReaderContainer) {
            hideCurrent();
        }
        current = articleMetaContainer;
        beginMetaDataLoad();
        showCurrent();
    }

    function enableEditorMode() {
        editorModeButton.addClass("disabled");
        metaModeButton.removeClass("disabled");
        readerModeButton.removeClass("disabled");
        if (current != articleReaderContainer) {
            hideCurrent();
        }
        current = articleEditorContainer;
        showCurrent();
    }

    // hook up the reader / meta / editor buttons
    readerModeButton.click(enableReaderMode);
    metaModeButton.click(enableMetaMode);
    editorModeButton.click(enableEditorMode);

    wikidown.article = {};
    wikidown.article.editor = editor;
    wikidown.article.metadata = metadata;

})(window.wikidown = window.wikidown || {}, jQuery)
