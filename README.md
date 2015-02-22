# \[wiki\]\(//down\)

## The wiki that doesn't suck

wiki-down is a markdown based wiki built on .NET and MongoDB. 

###Features

* One markdown document - many outputs
* Markdown renders the same at the client and the server. Editor previews 
should match the real output!
* Rendered article content produced before it's requested
* Auditing and article history

###System Components

* ASP.NET Content Server
    * MVC web presentation
    * WebAPI - read data / edit articles
* MongoDB database
    * Articles - history, drafts, trash
    * System - logs, audits, configuration
* WPF System Administration Tool
    * Manage system configs
    * Monitor system health

### We we're at

Mid afternoon, day zero. Almost all the layers in are in place, one more decision on locating the
transformation engine and we're done for 1.0 architecture.


