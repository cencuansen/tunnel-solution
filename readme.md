# 加密通信
## 结构
|项目|名称|地址|说明|
|-|-|-|
|Client1|客户端|自动|向目标服务发起请求|
|SocksR-Local|代理本地客户端|http://127.0.0.1:8001|接受Client1请求，加密数据，向SocksR-Remote进行请求|
|SocksR-Remote|代理远程服务端|http://127.0.0.1:8002|接受SocksR-Local请求，解密数据，向Target1进行请求|
|Target1|目标服务|http://127.0.0.1:8003|接受SocksR-Remote请求，做出响应|
