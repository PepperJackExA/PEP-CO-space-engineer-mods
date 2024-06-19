const app = Vue.createApp({
  data() {
    return{
      // importObject:"Me"
      importObject: null, //Full import file as JSON object
      blocks: null, //Block section of the import file
      search: "", //Search string for block name
      selectedBlocks: [], //Array for the blocks that were selected as part of the planner
      componentList: [],
    }
  },
  methods: {
    onGameFileUpload(e) {
      var files = e.target.files || e.dataTransfer.files;
      if (!files.length)
        return;
      this.parseFile(files[0]);
    },
    parseFile(file) {
      var reader = new FileReader();
      var vm = this;

      reader.onload = (e) => {
        try {
          vm.importObject = JSON.parse(e.target.result)
          vm.blocks = vm.importObject.blocks
        } catch (error) {
          console.log(error)
          vm.importObject = null
          vm.blocks = null
        }
      };
      reader.readAsText(file);
    },
    addToSelection(e,block){
      console.log(e)
      console.log(block.displayName)
      var amount = 1
      
      var indexLookup = this.selectedBlocks.findIndex((selectedBlock) => {
        return selectedBlock.displayName == block.displayName
      })
      // remove soon: console.log(indexLookup)
      if (indexLookup != -1){
        this.selectedBlocks[indexLookup] = {
          "displayName":block.displayName,
          "amount":amount + this.selectedBlocks[indexLookup].amount
        }
      }
      else{
        var addedBlock = {
          "displayName":block.displayName,
          "amount":amount
        }
        this.selectedBlocks.push(addedBlock)
      }
    },
    removeFromSelection(e,block,index){
      this.selectedBlocks.splice(index,1)
    },
    calculateComponentList(){
      this.componentList = []
      //Take the blocks in selectedBlocks, iterate them, check if they already exist in the componentList and add the corresponding amount of components to the componentList
      this.selectedBlocks.forEach(selectedBlock => {

        var searchBlockName = selectedBlock.displayName
        //Create a variable with the number of blocks in the selection list
        var selectedBlockAmount = selectedBlock.amount
        //Lookup the block from the defaultBlockList (blocks)
        var defaultBlock = this.blocks.find((myBlock) => {
          return myBlock.displayName.match(searchBlockName)
        })
        //Create a variable with the components
        var components = defaultBlock.components

        //Iterate the components
        components.forEach(componentfromBlock => {
          
          var currentComponentID = componentfromBlock.componentID
          console.log("currentComponentID: "+currentComponentID)
          //Create variable with the amount of components required for this entry
          var currentComponentAmount = componentfromBlock.amount
          //Check if component already exists in the componentList
          var indexLookup = this.componentList.findIndex((mycomponent) => {
            return mycomponent.componentID.match(currentComponentID)
          })
          console.log("indexLookup: "+indexLookup)
          // remove soon: console.log(indexLookup)
          if (indexLookup != -1){ //Component already exists in the List so we replace the current one and sum the components
            this.componentList[indexLookup] = {
              "componentID":currentComponentID,
              "amount": this.componentList[indexLookup].amount + (currentComponentAmount*selectedBlockAmount)
            }
          }
          else{ //Component doesn't exist so we create the entry from scratch
            var addedComponents = {
              "componentID":componentfromBlock.componentID,
              "amount":currentComponentAmount*selectedBlockAmount
            }
            this.componentList.push(addedComponents)
            console.log(this.componentList)
          }
          
        });
      });
    }
  },
  computed: {
    filteredBlocks: function(){
      if(this.blocks != null) {
        return this.blocks.filter((block) => {
          return block.displayName.match(this.search)
      })
      }
      else{
        return null
      }
    }
  },
  created() {
    this.$watch("selectedBlocks", (newSelectedBlocks) => {
      this.calculateComponentList()
    }, { deep: true });
  }
})

app.mount('#app')