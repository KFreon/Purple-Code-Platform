<style src="./searchableSelect.css"></style>

<script lang='ts'>
	import type { SearchableItem } from "$lib/data";

  export let options: SearchableItem[];
  export let selectedItem: SearchableItem | undefined;
  export let placeholder: string | undefined;
  export let onItemSelected: (item: SearchableItem) => void;

  let searchText: string | undefined;
  let isOpen: boolean = false;

  let popupSearchBox: HTMLElement | undefined;
</script>

<div class='searchable-select'>
  <button on:click={() => {
    isOpen = !isOpen;
    setTimeout(() => {
      popupSearchBox?.focus();
    }, 1);
  }} class='inline-button'>{selectedItem?.name ?? options?.[0]?.name ?? "unknown"}
  </button>
  <div class='popout-options' style={isOpen? 'display: block' : ''}>
    <input bind:this={popupSearchBox} type='text' bind:value={searchText} placeholder={placeholder}/>
    <div class='scrollable-container'>
      <ul>
        {#each options.filter(x => !searchText || x.name.includes(searchText)) as option}
          <!-- svelte-ignore a11y-click-events-have-key-events -->
          <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
          <li value={option.value} on:click={() => {
            onItemSelected(option);
            isOpen = false;
            searchText = '';
          }}>{option.name}</li>
        {/each}
      </ul>
    </div>
  </div>
</div>