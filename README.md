# SerilogTest
## Setting up Elasticsearch + Kibana
- Spin up containers with `docker-compose up`
- Go to http://localhost:5601
- Restart API so indexes are created
- D -> Manage spaces > Kibana > Data Views > Create > `serilogtest-*`
- Click on hamburger menu > Discover