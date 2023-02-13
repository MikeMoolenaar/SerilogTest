# SerilogTest
Experimental logging stack. The main goal is to log exceptions, make them searchable and create alerts.  

Technologies used:
- ASP.NET Core api
- [Serilog](https://github.com/serilog/serilog) logging library for .NET
- [Elasticsearch + Kibana](https://github.com/elastic/elasticsearch) store logs and make them searchable via a dashboard
- [Praeco](https://github.com/johnsusek/praeco) alerting tool for Elasticsearch, also includes a web interface

## Installation
- Spin up containers with `docker-compose up`
- Go to http://localhost:5601
- Restart API so indexes are created
- D -> Manage spaces > Kibana > Data Views > Create > `serilogtest-*`
- Click on hamburger menu > Discover
