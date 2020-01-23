<template>
  <div class="hello">
    <h1>{{ msg }}</h1>
    <p>
      For a guide and recipes on how to configure / customize this project,<br>
      check out the
      <a href="https://cli.vuejs.org" target="_blank" rel="noopener">vue-cli documentation</a>.
    </p>
    <button @click="sendAutReq" class="btn">get token</button>
    <br/>
     <button @click="sendValidateReq" class="btn">Validate token</button>
  </div>
</template>

<script>
  var model = {Username:"test", Password:"test"}
export default {
  name: 'HelloWorld',
  props: {
    msg: String
  },
  methods:{
    sendAutReq(){
      var  dispatch = this.$store.dispatch;
      var request = new XMLHttpRequest();
      request.open('POST', "http://localhost:58832/users/Authenticate", true);
      request.setRequestHeader('Content-Type', 'application/json');
      request.setRequestHeader('Accept', 'application/json');
      request.setRequestHeader('Access-Control-Allow-Origin','*');
    
      request.send(JSON.stringify(model));

      request.onreadystatechange = function(){
        if(request.readyState === 4){
          var response = JSON.parse(request.responseText)
          var accsess_token = response.token;
          var username = response.username;
          alert(JSON.stringify(response)+"  "+username+"  "+accsess_token)
          dispatch("putToken" ,{username,token:accsess_token} );
        }
      }
    },
    sendValidateReq(){
    var accsess_token = this.$store.state.tokens[model.Username];
     var request0 = new XMLHttpRequest();
            request0.open('GET', "http://localhost:58832/users/GetAll", true);
            request0.setRequestHeader('Access-Control-Allow-Origin','*');
            request0.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
            request0.setRequestHeader('Accept', '*/*');
            request0.setRequestHeader('Authorization', 'Bearer ' +accsess_token);
            request0.send();

            request0.onreadystatechange = function(){
              if(request0.readyState === 4){
                alert(request0.responseText+ "  "+ request0.status);
              }
            }
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
.btn{
  width: 100px;
  height: 50px;
  text-align: center;
}
</style>
