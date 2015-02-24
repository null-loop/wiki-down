# **\[wiki\]**\(//down\)

## The wiki that doesn't suck

wiki-down is a markdown based wiki built on .NET and MongoDB. There might be some NodeJS - the juries still
out...

###Features

* One content store - many sites
* One markdown document - many outputs & formats
* Markdown renders the same at the client and the server. Editor previews 
should match the real output!
* Rendered article content produced before it's requested
    * Article markdown to article specific formats
    * Site and article templates integrate article content
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

####0.1 - Article lifecycle & ancillary services
####0.2 - Article transformations
####0.3 - Site structure & article templating
####0.4 - User management, authentication, and authorisation
####0.5 - Article & configuration security attributes
####0.6 - Admin console & system monitors

###`Current target: 0.1 - Article lifecycle`

####Done

* Draft article create and revise
* Publish draft article
* Trash article
* Recover trashed article
* Record article history
* Writing to system logs
* Writing to system audit
* Working (but a bit hacky) MongoDB / JS markdown transformation

####TODO

* Investigate MongoDB / JS integration vs. NodeJS 'shards' for markdown transformation
* Efficient generated article content retrieval
* Editor / API integration
* Article search and retrieval
* Setup main website - running on latest bits
* Setup sandbox website with auto-cleardown - again on latest bits
* Investigate Azure package deployment