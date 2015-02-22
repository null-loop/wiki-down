# \[wiki\]\(//down\)

## The wiki that doesn't suck

wiki-down is a markdown based wiki built on .NET and MongoDB. 

###Features

* One markdown document - many outputs
* One content store - many websites
* Markdown renders the same at the client and the server. Editor previews 
should match the real output!
* Rendered article content produced before it's requested
* Auditing and article history

###System Components

* ASP.NET Content Server
    * MVC web presentation
        * Site & article navigation
        * Read-only system monitors
    * WebAPI 
        * Read / write articles
        * Read system data
* MongoDB database
    * Articles - history, drafts, trash
    * System - logs, audits, configuration
* WPF System Administration Tool
    * Manage system configs
    * Monitor system health

###Milestones

####0.1 - Article lifecycle
####0.2 - Article transformations
####0.3 - Site structure & article templating

**`Current target: 0.1 - Article lifecycle`**

####Done

* Draft article create and revise
* Publish draft article
* Trash article
* Recover trashed article
* Record article history
