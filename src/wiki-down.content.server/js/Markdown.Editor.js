(function (editor, $, undefined) {

    var dialogPadding = 40;

    function calculateDialogHeight() {
        return $(window).height() - dialogPadding;
    }

    function calculateDialogWidth() {
        return $(window).width() - dialogPadding;
    }

    function calculateWindowHeight() {
        return calculateDialogHeight() / 2;
    }

    function markupMarkdown(markdownContent) {
        return marked(markdownContent);
    }

    function articleViewModel(articleData) {

        var content = articleData.Markdown;

        this.articleData = articleData;
        this.articleGlobalId = ko.observable(articleData.GlobalId);
        this.articlePath = ko.observable(articleData.Path);
        this.articleTitle = ko.observable(articleData.Title);
        this.parentArticlePath = ko.observable(articleData.ParentArticlePath);
        this.allowChildren = ko.observable(articleData.AllowChildren);
        this.draft = ko.observable(articleData.Draft);
        this.indexed = ko.observable(articleData.Indexed);
        this.originalMarkdownContent = ko.observable(content);
        this.edittedMarkdownContent = ko.observable(content);
        this.currentMarkdownContent = ko.observable(content);
        this.windowHeight = ko.observable(calculateWindowHeight());
        this.textAreaHeight = ko.observable(calculateWindowHeight() - 100);
        this.previewAreaHeight = ko.observable(calculateWindowHeight() - 84);
        
        this.markedupContent = ko.pureComputed(function() {
            return markupMarkdown(this.currentMarkdownContent());
        }, this);

        this.addText = ko.pureComputed(function() {
            var ac = this.addCount();
            var s = ac != 1 ? 's' : '';
            return "Added " + ac + " line" + s;
        }, this);

        this.deleteText = ko.pureComputed(function() {
            var ac = this.deleteCount();
            var s = ac != 1 ? 's' : '';
            return "Deleted " + ac + " line" + s;
        }, this);

        this.changedText = ko.pureComputed(function () {
            var ac = this.changedCount();
            var s = ac != 1 ? 's' : '';
            return "Changed " + ac + " line" + s;
        }, this);

        this.addCount = ko.observable(0);
        this.deleteCount = ko.observable(0);
        this.changedCount = ko.observable(0);

        this.anyChanges = ko.pureComputed(function() {
            return this.addCount() > 0 ||
                this.deleteCount() > 0 ||
                this.changedCount() > 0;
        },this);

        this.originalActive = ko.observable(false);
        this.edittedActive = ko.observable(true);
        this.diffActive = ko.observable(false);

        this.articleLayout = ko.observable(false);
        this.pageLayout = ko.observable(false);
        this.markupLayout = ko.observable(true);

        this.viewOriginal = function() {
            this.originalActive(true);
            this.edittedActive(false);
            this.diffActive(false);
            if (!this.previewActive() && this.previewWasActive) this.showPreview();
            this.edittedMarkdownContent(this.currentMarkdownContent());
            this.currentMarkdownContent(this.originalMarkdownContent());
        };

        this.edit = function() {
            this.originalActive(false);
            this.edittedActive(true);
            this.diffActive(false);
            if (!this.previewActive() && this.previewWasActive) this.showPreview();
            this.currentMarkdownContent(this.edittedMarkdownContent());
        };

        this.diff = function () {
            this.diffActive(true);
            this.originalActive(false);
            this.edittedActive(false);
            this.previewActive(false);
        }

        this.previewActive = ko.observable(true);
        this.previewWasActive = true;

        this.togglePreview = function() {
            if (this.previewActive()) this.hidePreview();
            else this.showPreview();
        }

        this.showPreview = function () {
            this.previewActive(true);
            this.previewWasActive = true;
        };

        this.hidePreview = function () {
            this.previewActive(false);
            this.previewWasActive = false;
        };

        this.viewArticleLayout = function() {
            this.articleLayout(true);
            this.pageLayout(false);
            this.markupLayout(false);
        };

        this.viewPageLayout = function () {
            this.articleLayout(false);
            this.pageLayout(true);
            this.markupLayout(false);
        };

        this.viewMarkupLayout = function () {
            this.articleLayout(false);
            this.pageLayout(false);
            this.markupLayout(true);
        };
    }

    function open(contentArticleId) {
        $.get(window.wikidown.uri('api/article/g/' + contentArticleId), function(content) {
            editor.dialogDom.dialog('open');
            editor.article = new articleViewModel(content);
            editor.article.markedupContent.subscribe(function (newValue) {
                //hljs.highlightBlock(editor.htmlPreviewDom);
            });
            //hljs.highlightBlock(editor.htmlPreviewDom);
            ko.applyBindings(editor.article, editor.dom.get(0));
        });
    }

    function init() {
        $.get(window.wikidown.uri('html/wiki.down.editor.dialog.html'), function (content) {
            var contentDom = $(content);
            var editorDom = contentDom.find('#dialogContent');
            editor.htmlPreviewDom = editorDom.find('#htmlPreview');
            editor.dom = editorDom;
            editor.dialogDom = editorDom.dialog({
                autoOpen: false,
                closeOnEscape: false,
                closeText:'x',
                draggable: false,
                modal: true,
                resizable: false,
                position: { my: "center", at: "center", of: window },
                show: { effect: "fade", duration: 500 },
                hide: { effect: "fade", duration: 500 },
                height: calculateDialogHeight(),
                width: calculateDialogWidth(),
                buttons: [{
                    text: "Save",
                    click: function () {

                    },
                    class:"btn btn-primary"
                },{
                    text: "Cancel",
                    click: function () {
                        if (editor.article.anyChanges()) {
                            // confirm lose!
                            editor.dialogDom.dialog('close');
                        } else {
                            editor.dialogDom.dialog('close');
                        }
                    },
                    class:"btn btn-danger"
                }]
            });

            

            //TODO:Define viewmodel
            //TODO:Instantiate viewmodel
            //TODO:Wire up knockout!
        });
        $.get(window.wikidown.uri('editor/templates/page'), function(content) {
            var contentDom = $(content);
            editor.pageTemplateDom = contentDom;
        });
        $.get(window.wikidown.uri('editor/templates/article'), function(content) {
            var contentDom = $(content);
            editor.articleTemplateDom = contentDom;
        });
    }

    editor.open = open;

    init();

})(window.wikidown.editor = window.wikidown.editor || {}, jQuery)