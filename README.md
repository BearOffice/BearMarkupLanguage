# ConfigReadingLib
 A class library to read or modify the config file reliably.  
  
# Memo
 Create a new config file.  
 ```
 ConfigReader.Create(new ConfigInfo[]  
 {  
     new ConfigInfo { Key = "Key A", Value = "Value A" },  
     new ConfigInfo { Key = "Key B", Value = "Value B", Comment = "Comment B" }  
 }, "myapp.conf");  
 ```
  
 Read a config file.  
 `var reader = new ConfigReader("myapp.conf");`  
  
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
  
 Add new keys.  
 ```
 reader.AddConfig(new ConfigInfo[]  
 {  
     new ConfigInfo{Key = "Key C" , Value = "Value C"},  
     new ConfigInfo {Key = "Key D" , Value ="Value D" , Comment =  "Comment B"}  
 });  
 ```
  
 Set all properties value contained within the class specified from the existent keys.  
 ```
 public int FontSize { get; set; }  
 public double FontOpacity { get; set; }  
 ```
 ```
 reader.SetPropertiesFromKeys(this, new ParseFromString  
 {  
     [typeof(double)] = x => double.Parse(x)  // Default contains the following parsing rules: 'int', 'string'.  
 });  
 ```
  
 Save all properties value contained within the class specified to the existent keys.  
 ```
 reader.SavePropertiesToKeys(this, new ParseToString  
 {  
     [typeof(double)] = x => x.ToString()  
 });  
 ```
