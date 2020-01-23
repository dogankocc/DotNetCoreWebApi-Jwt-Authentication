

 import Vue from 'vue'
 import Vuex from 'vuex'
 
 Vue.use(Vuex)

 export const store = new Vuex.Store({
    state: {
        data: {
           
        },
        tokens: {}
      },
    mutations: {
        PUT_TOKEN(state,  payload) {
            state.tokens[payload.username] = payload.token;
            //alert(JSON.stringify(state.tokens))
          },
          
    },
    actions : {
        putToken(context, payload) {
          context.commit("PUT_TOKEN", payload);
        }
      },
   
  }) 