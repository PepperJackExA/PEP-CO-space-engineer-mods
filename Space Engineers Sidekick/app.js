const app = Vue.createApp({
  data() {
    return {
      isLoaded: false,
      isDragging: false,
      importObject: localStorage.getItem("importObject") ? JSON.parse(localStorage.getItem("importObject")) : null,
      search: "", //Search string for block name
      sizeFilter: "large/small",
      selectedBlocks: localStorage.getItem("selectedBlocks") ? JSON.parse(localStorage.getItem("selectedBlocks")) : [], //Array for the blocks that were selected as part of the planner
      componentList: [],
      totalVolume: 0,
      totalWeight: 0,
      visible1: false,
      visible2: false,
      visible3: false,
      visible4: false,
    }
  },
  methods: {
    handleDragOver(event) {
      // console.log("this.isDragging = ", this.isDragging);
      event.preventDefault();
      this.isDragging = true;
    },
    handleDragLeave(event) {
      event.preventDefault();
      this.isDragging = false;
    },
    handleDrop(event) {
      event.preventDefault();
      this.isDragging = false;
      const files = event.dataTransfer.files;
      this.handleFiles({ target: { files } });
    },
    handleFiles(event) {
      // console.log('Files:', event.target.files);
      const files = event.target.files;
      this.parseFile(files[0]);
    },
    parseFile(file) {
      var reader = new FileReader();

      reader.onload = (e) => {
        try {
          this.importObject = JSON.parse(e.target.result)
          localStorage.setItem('importObject', JSON.stringify(this.importObject));
        } catch (error) {
          console.log(error)
          this.importObject = null
        }
      };
      reader.readAsText(file);
    },
    addToSelection(e, block) {
      // console.log(e)
      // console.log(block.displayName)
      var amount = 1

      var indexLookup = this.selectedBlocks.findIndex((selectedBlock) => {
        return selectedBlock.displayName == block.displayName
      })
      // console.log(indexLookup)
      if (indexLookup != -1) {
        this.selectedBlocks[indexLookup] = {
          "displayName": block.displayName,
          "size": block.size,
          "amount": amount + this.selectedBlocks[indexLookup].amount
        }
      }
      else {
        var addedBlock = {
          "displayName": block.displayName,
          "size": block.size,
          "amount": amount
        }
        this.selectedBlocks.push(addedBlock)
      }
      // 3. Sort the selectedBlocks array alphabetically by displayName
      this.selectedBlocks.sort((blockA, blockB) => {
        const nameA = blockA.displayName.toLowerCase(); // Case-insensitive sorting
        const nameB = blockB.displayName.toLowerCase();
        return nameA < nameB ? -1 : nameA > nameB ? 1 : 0;
      });
    },
    decrementBlockAmount(event, block, index) {
      // Use the event, block, and index here
      if (block.amount > 0) {
        block.amount--;
      } else {
        this.selectedBlocks.splice(index, 1)
      }
    },
    removeFromSelection(e, block, index) {

      var indexLookup = this.selectedBlocks.findIndex((selectedBlock) => {
        return selectedBlock.displayName == block.displayName
      })
      // console.log(block)
      // console.log(indexLookup)
      this.selectedBlocks.splice(indexLookup, 1)
    },
    calculateComponentList() {
      this.componentList = []
      //Take the blocks in selectedBlocks, iterate them, check if they already exist in the componentList and add the corresponding amount of components to the componentList
      this.selectedBlocks.forEach(selectedBlock => {

        var searchBlockName = selectedBlock.displayName
        //Create a variable with the number of blocks in the selection list
        var selectedBlockAmount = selectedBlock.amount
        //Lookup the block from the defaultBlockList (blocks)
        var defaultBlock = this.importObject.blocks.find((myBlock) => {
          return myBlock.displayName.match(searchBlockName)
        })
        //Create a variable with the components
        var components = defaultBlock.components

        //Iterate the components
        components.forEach(componentfromBlock => {

          var currentComponentID = componentfromBlock.componentID
          // console.log("currentComponentID: "+currentComponentID)
          //Create variable with the amount of components required for this entry
          var currentComponentAmount = componentfromBlock.amount
          //Check if component already exists in the componentList
          var indexLookup = this.componentList.findIndex((mycomponent) => {
            return mycomponent.componentID.match(currentComponentID)
          })
          // console.log("indexLookup: "+indexLookup)
          // remove soon: console.log(indexLookup)
          if (indexLookup != -1) { //Component already exists in the List so we replace the current one and sum the components
            this.componentList[indexLookup] = {
              "componentID": currentComponentID,
              "componentDisplayName": componentfromBlock.displayName,
              "amount": this.componentList[indexLookup].amount + (currentComponentAmount * selectedBlockAmount)
            }
          }
          else { //Component doesn't exist so we create the entry from scratch
            var addedComponents = {
              "componentID": componentfromBlock.componentID,
              "componentDisplayName": componentfromBlock.displayName,
              "amount": currentComponentAmount * selectedBlockAmount
            }
            this.componentList.push(addedComponents)
            // console.log(this.componentList)
          }

        });
      });
    },
    calculateComponentValue() {
      this.totalVolume = 0
      this.totalWeight = 0
      this.componentList.forEach(component => {
        var componentLookup = this.importObject.components.find(element => {
          return element.componentID.match(component.componentID)
        })
        var componentVolume = componentLookup.volume
        var componentWeight = componentLookup.weight
        var componentAmount = component.amount
        this.totalVolume += componentVolume * componentAmount
        this.totalWeight += componentWeight * componentAmount
      });
    },
    clearInput() {
      this.totalVolume = 0
      this.totalWeight = 0
      this.componentList = []
      this.selectedBlocks = []
      this.importObject = null
      localStorage.setItem('importObject', JSON.stringify(this.importObject));
      localStorage.setItem('selectedBlocks', JSON.stringify(this.selectedBlocks));
    },
    componentDisplay(block){
      var displayString = []
      var index = 0
      block.components.forEach(component => {
        displayString.push(component.displayName + ": " + (component.amount + (index <= block.isCritical ? " (functional)" : "")))
          index++
        })
        displayString = displayString.join("\n")
        if (block.modContext != "()") {
          displayString += "\nMod: "+block.modContext
        }
        else displayString += "\nMod: Vanilla";
        return displayString
    },
    toggleSizeFilter() {
      if (this.sizeFilter === "large/small") {
        this.sizeFilter = "large";
      } else if (this.sizeFilter === "large") {
        this.sizeFilter = "small";
      } else {
        this.sizeFilter = "large/small"; // Toggle back to empty string
      }
    }
  },
  computed: {
    filteredBlocks: function () {
      if (this.importObject.blocks != null) {
        return this.importObject.blocks.filter((block) => {
          const displayNameMatches = block.displayName.toUpperCase().includes(this.search.toUpperCase());
          const sizeMatches = this.sizeFilter === "large/small" || block.size.toUpperCase() === this.sizeFilter.toUpperCase();
          return displayNameMatches && sizeMatches;
        });
      } else {
        return null;
      }
    }
  },
  created() {
    this.$watch("selectedBlocks", (newSelectedBlocks) => {
      this.calculateComponentList(),
        localStorage.setItem('selectedBlocks', JSON.stringify(this.selectedBlocks));
    }, { deep: true });
    this.$watch("componentList", (newComponentList) => {
      this.calculateComponentValue()
    }, { deep: true });
    this.calculateComponentList()
  }
});

app.mount('#app')
