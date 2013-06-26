# QScraper 2.0
## Specifications for the next-generation QScraper
###### Draft 1.2

## Overview

QScraper originally started as a simple 'script-like' program that would perform its task and save a single data file (.json) recording the results. This approach is inefficient, restrictive, and difficult to build around and as such has prompted the need for QScraper 2.0 to take shape.

### Platform
#### Azure vs AWS vs Heroku
Currently the Platform of choice looks to be Azure. Amazon Web Services and Heroku have both been appealing but have particular drawbacks that I would like to avoid.

On Azure we would utilize the offered 'Cloud Services' and 'Mobile Services' products. With Cloud Services we would build our various scraper modules (explained below) and the API service layer into one 'app'.

### Programming Language
#### .NET vs NodeJS
Currently .NET is appealing due to the power of JSON.NET and the familiarity with .NET (versus say Ruby). Alternatively, if we wanted to be super hipster we could go the NodeJS route, but having you learn Javascript (NodeJS) from scratch would not be a quick task.

### Requirements
1. Provide a API for all future Deal Flux apps to utilize
2. Provide Push Notifications to client apps
3. Be capable of hosting a static informational website
4. Be highly scalable
5. Be highly configurable (Ex. all responses would be GZIP compressed)
6. Be modular, allowing for each deal source module that we create to be 'dropped' in and automatically added to the list of services available through the API.
7. Be lightweight, as per #3, this needs to be lightweight and scalable. While hardware (Azure) can compensate for poor performance of an application to some extent, it will be costly and due to the mass amount of requests that Deal Flux client apps will generate this needs to be able to handle thousands of requests every few minutes (just as a general benchmark).
8. Be Restful Compliant
9. Support for language/region-designated results
10. Perserve description formatting.

## Outline of API Structure
Currently the idea is to break each 'deal source' into their own API route and then provide a consistent set of options/commands that are available on any given deal source API route/request. Furthermore there should be an available bulk request.

`dealfluxapp.com/v2/{deal-source-name}/{deal-title}/`

##### v2
Just present as a basic form of versioning for the API allowing us to easily and quickly make significant changes in future API rollouts if need be while still maintaining full support for previous apps (non-upgraded apps)

##### deal-source-name
This would be a all lower case, no special character, and hyphenated name for the deal source.

Ex. 'woot', '1saleaday', 'yugster', etc.

##### deal-title
This would be a specified category name within a given deal source. For example on [Woot](http://www.woot.com/) there are various different deal pages/categories.

Ex. 'tech', 'home', 'tools-and-garden', etc.

These would be different per each deal source as no two deal sources have the same amount or types of pages/categories.

## Deal Sources Supported
##### Woot!
http://api.woot.com/2
##### 1SaleADay
http://feeds.feedburner.com/1saleaday?format=xml
##### Yugster
http://www.yugster.com/rss.xml
##### DailySteals
http://www.dailysteals.com/rss
##### Dell Daily Deals
http://accessories.dell.com/sna/sna.aspx?c=ca&cs=cadhs1&l=en&s=dhs&~topic=hotdeals_redhot

- TeeFury
- NeweggFlash
- Amazon 
- - Gold Box Deals (include upcoming deals http://www.amazon.com/gp/goldbox/all-deals?ie=UTF8&gbNodeId=468642)
- - Amazon Featured Deals (Video Games)





