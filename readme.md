# NeDB分割工具

> 这是一个简单的版本，初学C#试做的。本来想做的更好一点，就想一个数据库的客户端的样子，后来看了一遍文章《不要为了编程而编程》，大概意思是功能实现，简单方便就好，不要为了一堆看似很强大实际没用处的功能而浪费时间和精力。故，可能没有下个版本了。

## 下载教程  直接下载后编译或者我将提供一个[下载链接](https://yanlongli.oss-cn-shanghai.aliyuncs.com/download/nedb%E5%88%86%E7%A6%BB%E5%B7%A5%E5%85%B7.exe)

## 使用教程

Windows系统运行exe文件，将自动在同级目录创建配置文件。用户保存配置信息。第一次请点击_选择原始数据_-> 读取数据-> 第一个表格将展示原始数据及编号，第二个数据显示可供选择的字段\*1,可在第二个选择框中勾选需要进行筛选的字段，并在第三个表单中单机或双击，输入筛选值*2

注意：
	*1 仅1维可选 如{"name":"Yanlongli","domain":{"www":"www.yanlongli.com","blog":"blog.yanlongli.com"}}
	name字段是一级可选，domain有子集不可筛选
	*2 筛选条件为 并 （AND） ，并且必须有值，为空则无效，意味着不进行任何筛选。（这是一个BUG，不想改了）
![](https://i.imgur.com/zQOmyI4.png)