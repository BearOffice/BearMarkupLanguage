# ConfReaderLib
 A class library to read or modify the config file reliably.  
  
# Memo
 Create a new conf file.  
 `ConfReader.Create(new[] { ("Key A", "Value A", ""), ("Key B", "Value B", "Comment B") }, "myapp.conf");`  
  
 Read a config file.  
 `var reader = new ConfReader("myapp.conf");`  
  
 Get key's value.  
 `reader.GetValue("Key A");`  
>Value A  
  
 Try to get the key's value. This method returns true while the key specified exists.  
 `reader.TryGetValue("Key A", out string value);`  
>True  
  
 Change a key's value.  
 ```
 reader.ChangeValue("Key A", "New Value A");  
 reader.GetValue("Key A");  
 ```
>New Value A  
  
 Add a key.  
 `reader.AddConf(new[] { ("Key C", "Value C", ""), ("Key D", "Value D", "") });`  
  
 Set the class's properties specified automatically(directly).  
 ```
 public int FontSize { get; set; }  
 public double FontOpacity { get; set; }  
 ```
 ```
 var rule = new ParseFromString()  
 {  
     [typeof(double)] = x => double.Parse(x),  // Default contains the rules of the type 'int' and 'string'.  
 };  
 reader.SetProperties(this, rule);  
 ```
  
 Save the class's properties specified to the config file automatically(directly).  
 ```
 var rule = new ParseToString()  
 {  
     [typeof(double)] = x => x.ToString(),  
 };  
 reader.SaveProperties(this, rule, strict: true);  
 ```