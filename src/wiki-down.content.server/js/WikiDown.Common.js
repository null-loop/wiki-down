(function (wikidown, $, undefined) {

    function addMissingClass(target, element, css) {
        $(target).find(element).not("." + css).addClass(css);
    }

    function initMarkdownCss(target) {
        addMissingClass(target, "table", "table");
        addMissingClass(target, "button", "btn");

        // wire up article edit buttons
        $("button[data-edit-article-id]").click(function () {
            var cl = $(this);
            var id = cl.data("edit-article-id");
            window.wikidown.editor.open(id);
        });

        // wire up article view buttons
    }

    function rootUri(uri) {
        return wikidown.root + uri;
    }

    function overlay(message, element) {
        
    }

    function hideOverlay(element) {
        
    }

    wikidown.css = initMarkdownCss;
    wikidown.root = "/";
    wikidown.uri = rootUri;
    wikidown.overlay = overlay;
    wikidown.hideOverlay = hideOverlay;

})(window.wikidown = window.wikidown || {}, jQuery)
